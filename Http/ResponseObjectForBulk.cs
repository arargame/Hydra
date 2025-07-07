namespace Hydra.Http
{
    public class ResponseObjectForBulk : ResponseObject
    {
        public List<Guid> SucceededIds { get; set; } = new();
        public List<Guid> FailedIds { get; set; } = new();
        public List<string> FailedMessages { get; set; } = new();

        public ResponseObjectForBulk AddSuccess(Guid id)
        {
            SucceededIds.Add(id);
            return this;
        }

        public ResponseObjectForBulk AddFailure(Guid id, string errorMessage)
        {
            FailedIds.Add(id);
            FailedMessages.Add($"[{id}] - {errorMessage}");

            Messages.Add(new ResponseObjectMessage(
                title: $"Entity ID: {id}",
                text: errorMessage,
                showWhenSuccess: false));

            return this;
        }

        public new ResponseObjectForBulk SetSuccess(bool success)
        {
            base.SetSuccess(success);
            return this;
        }

        public new ResponseObjectForBulk SetActionName(string? actionName)
        {
            base.SetActionName(actionName);
            return this;
        }

        public new ResponseObjectForBulk UseDefaultMessages()
        {
            base.UseDefaultMessages();
            return this;
        }

        public new ResponseObjectForBulk AddExtraMessage(ResponseObjectMessage message)
        {
            base.AddExtraMessage(message);
            return this;
        }

        public new ResponseObjectForBulk AddExtraMessages(List<ResponseObjectMessage> messages)
        {
            base.AddExtraMessages(messages);
            return this;
        }

        public new ResponseObjectForBulk SetData(object data)
        {
            base.SetData(data);
            return this;
        }
    }
}



