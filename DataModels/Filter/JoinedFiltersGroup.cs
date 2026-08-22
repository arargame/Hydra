using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public interface IJoinFilter : IQueryableFilter
    {
        //NOT: Operandlar IFilter değil IQueryableFilter'dır. Bir JoinedFiltersGroup'un kendisi de
        //IQueryableFilter olduğu için, bir grup başka bir grubun operandı olabilir — yani filtreler
        //istenildiği kadar iç içe geçebilir. (Eskiden bunlar IFilter idi ve grup IFilter'ı
        //uygulamadığı için iç içe geçemiyordu; bu yüzden 3. filtre için AnotherFilter diye ayrı bir
        //alan eklenmişti ve 4.'sünde "3'ten fazla filtre bağlanamaz" hatası alınıyordu.)
        IQueryableFilter LeftFilter { get; set; }

        IQueryableFilter RightFilter { get; set; }

        IQueryableFilter? AnotherFilter { get; set; }

        FilterJoinType JoinTypeForAnother { get; set; }

        FilterJoinType JoinType { get; set; }
    }

    /// <summary>
    /// İki filtreyi (And/Or ile) birleştiren bileşik filtre. Kendisi de bir IQueryableFilter
    /// olduğundan operand olarak kullanılabilir; N filtre, sola yaslı bir ağaç hâlinde birleştirilir:
    ///
    ///     (((f0 And f1) And f2) And f3) ...
    ///
    /// Parametre numaralandırması kökten aşağı yayılır (bkz. SetStartParameterIndex), Parameters
    /// listesi ise çocukların parametrelerinin aynı sırayla düzleştirilmiş hâlidir. Listedeki
    /// nesneler çocuklarınkiyle AYNI referanslardır; bu yüzden kökte yapılan numaralandırma
    /// yapraklara da yansır ve üretilen SQL ile parametre sözlüğü tutarlı olur.
    /// </summary>
    public class JoinedFiltersGroup : QueryableFilter, IJoinFilter
    {
        public IQueryableFilter LeftFilter { get; set; }

        public IQueryableFilter RightFilter { get; set; }

        /// <summary>
        /// Eski API ile uyumluluk için korunan üçüncü operand. Artık SetFromColumns bunu
        /// kullanmıyor (iç içe gruplar bu ihtiyacı ortadan kaldırdı), ancak elle kurulan
        /// filtre ağaçları için çalışmaya devam eder.
        /// </summary>
        public IQueryableFilter? AnotherFilter { get; set; } = null;

        public FilterJoinType JoinTypeForAnother { get; set; }

        public FilterJoinType JoinType { get; set; }

        public int Priority { get; set; }

        public bool CreateFilterComponentFromThis { get; set; }

        public List<IFilterParameter> GetParameters
        {
            get
            {
                return Parameters;
            }
        }

        public JoinedFiltersGroup(IQueryableFilter leftFilter, IQueryableFilter rightFilter, FilterJoinType joinType)
            : this(leftFilter, rightFilter)
        {
            JoinType = joinType;
        }

        private JoinedFiltersGroup(IQueryableFilter leftFilter, IQueryableFilter rightFilter)
        {
            LeftFilter = leftFilter;

            RightFilter = rightFilter;

            Initialize();
        }

        public override void Initialize()
        {
            Parameters = new List<IFilterParameter>();

            //DİKKAT: Bu metot İKİ kez çalışır.
            //1) BaseObject'in constructor'ı virtual Initialize()'ı çağırır. C#'ta taban sınıfın
            //   constructor'ı türetilmiş sınıfınkinden ÖNCE koştuğu için o an LeftFilter/RightFilter
            //   henüz atanmamıştır (null). Eskiden burada NullReferenceException atılıyordu ve
            //   ikiden fazla filtre uygulanan her sorgu patlıyordu.
            //2) Türetilmiş constructor alanları atadıktan sonra Initialize()'ı tekrar çağırır —
            //   parametreleri asıl dolduran çağrı budur.
            if (LeftFilter == null || RightFilter == null)
                return;

            Parameters.AddRange(LeftFilter.Parameters);

            Parameters.AddRange(RightFilter.Parameters);

            //Kendi başlangıç indeksimizden itibaren ağacı yeniden numaralandır. Kök grup için bu
            //0'dır; iç içe kurulumda kök, en dıştaki grup tarafından tekrar numaralandırılır.
            SetStartParameterIndex(StartParameterIndex);
        }

        /// <summary>
        /// Numaralandırmayı çocuklara yayar: sol operand bu grubun başlangıcından, sağ operand ise
        /// solun bittiği yerden başlar. Çocuk da bir grup ise aynı mantık özyinelemeli sürer.
        /// </summary>
        public override IQueryableFilter SetStartParameterIndex(int index)
        {
            StartParameterIndex = index;

            if (LeftFilter == null || RightFilter == null)
                return this;

            LeftFilter.SetStartParameterIndex(index);

            RightFilter.SetStartParameterIndex(LeftFilter.FinishParameterIndex);

            if (AnotherFilter != null)
                AnotherFilter.SetStartParameterIndex(RightFilter.FinishParameterIndex);

            return this;
        }

        public JoinedFiltersGroup Bind(IQueryableFilter anotherFilter, FilterJoinType joinTypeForAnother)
        {
            if (AnotherFilter != null)
                throw new Exception("The joined filters group does not support to bind more than 3 filters.3rd one is already defined.Please join a new JoinFilter");

            AnotherFilter = anotherFilter;

            JoinTypeForAnother = joinTypeForAnother;

            AnotherFilter.SetRootFilter(this);

            Parameters.AddRange(AnotherFilter.Parameters);

            SetStartParameterIndex(StartParameterIndex);

            return this;
        }

        public override string PrepareQueryString()
        {
            var queryString = $"({LeftFilter?.PrepareQueryString()} {JoinType} {RightFilter?.PrepareQueryString()})";

            if (AnotherFilter != null)
                queryString += $" {JoinTypeForAnother} ({AnotherFilter.PrepareQueryString()})";

            return queryString;
        }

        /// <summary>
        /// Filtreli kolonlardan sola yaslı bir filtre ağacı kurar ve kökü döner.
        /// Filtre sayısı için bir üst sınır yoktur.
        ///
        /// Tek filtre varsa liste BOŞ döner — o durumda çağıran taraf zaten kolonun kendi
        /// filtresini doğrudan kullanır (bkz. QueryBuilder.SetTableFilter / Table.SetFilter).
        /// </summary>
        public static List<JoinedFiltersGroup> SetFromColumns(List<IMetaColumn> filteredColumns)
        {
            var joinedFiltersGroupList = new List<JoinedFiltersGroup>();

            var filters = filteredColumns?.Where(c => c?.Filter != null)
                                          .Select(c => (IQueryableFilter)c.Filter!)
                                          .ToList()
                          ?? new List<IQueryableFilter>();

            if (filters.Count < 2)
                return joinedFiltersGroupList;

            //Sola yaslı katla: ((f0 And f1) And f2) And f3 ...
            //Her adımda oluşan grup bir sonrakinin sol operandı olur.
            IQueryableFilter current = filters[0];

            for (var i = 1; i < filters.Count; i++)
            {
                current = new JoinedFiltersGroup(current, filters[i], FilterJoinType.And);
            }

            var root = (JoinedFiltersGroup)current;

            //Kök 0'dan başlar; numaralandırma bütün ağaca yayılır.
            root.SetStartParameterIndex(0);

            joinedFiltersGroupList.Add(root);

            return joinedFiltersGroupList;
        }
    }
}
