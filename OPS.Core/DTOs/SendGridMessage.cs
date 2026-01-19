using System.Collections.Generic;

namespace OPS.Core.DTOs;

public class SendGridMessage
{
    public List<SendGridMessagePersonalization> personalizations { get; set; }
    public List<SendGridMessageContent> content { get; set; }
    public SendGridMessageFrom from { get; set; }
    public SendGridMessageReplyTo reply_to { get; set; }
}

public class SendGridMessageTo
{
    public string email { get; set; }
    public string name { get; set; }
}

public class SendGridMessagePersonalization
{
    public List<SendGridMessageTo> to { get; set; }
    public string subject { get; set; }
}

public class SendGridMessageContent
{
    public string type { get; set; }
    public string value { get; set; }
}

public class SendGridMessageFrom
{
    public string email { get; set; }
    public string name { get; set; }
}

public class SendGridMessageReplyTo
{
    public string email { get; set; }
    public string name { get; set; }
}