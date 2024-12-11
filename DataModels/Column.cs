using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{

    public enum HtmlElementType
    {
        Input,
        InputToUploadFile,
        TextArea,
        DropdownList,
        RichTextEditor,
        DataList
    }

    public enum HtmlInputType
    {
        button,
        checkbox,
        color,
        date,
        datetime_local,
        email,
        file,
        hidden,
        image,
        month,
        number,
        password,
        radio,
        range,
        reset,
        search,
        submit,
        tel,
        text,
        time,
        url,
        week,
    }

    public enum ColumnValueType
    {
        Boolean,
        ByteArray,
        Char,
        Double,
        Enum,
        Float,
        Guid,
        Int,
        Object,
        String,
        DateTime
    }

    public interface IColumn : IBaseObject<IColumn>
    {
        ITable? Table { get; set; }

        string? Alias { get; set; }

        object? Value { get; set; }

        IColumn SetTable(ITable? table);

        IColumn SetAlias(string? alias);

        IColumn SetValue(object? value);
    }


    public abstract class Column : BaseObject<Column>,IColumn
    {
        public ITable? Table { get; set; }

        public IRow? Row { get; set; } = null;

        public string? Alias { get; set; } = null;

        public object? Value { get; set; } = null;

        public Column() { }

        public IColumn SetTable(ITable? table)
        {
            return this;
        }

        public IColumn SetRow(IRow row)
        {
            Row = row;

            return this;
        }

        public IColumn SetAlias(string? alias)
        {
            if (!string.IsNullOrWhiteSpace(alias) && alias.Contains(" "))
                throw new ArgumentException("Alias cannot contain spaces.");

            Alias = alias;

            return this;
        }

        public IColumn SetValue(object? value)
        {
            Value = value;

            return this;
        }
    }

}
