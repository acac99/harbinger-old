module harbinger.MessageDto
open System




[<CLIMutable>]
type Message =
    {
        Id : Guid
        Text: String
        CreatedAt: DateTime
        UpdatedAt: DateTime
    }
    

type MessageRequest =
    {
           Text: String
    }
    
    member this.GetMessage = {
        Id = Guid.NewGuid();
        Text = this.Text;
        CreatedAt = DateTime.Now
        UpdatedAt = DateTime.Now
    }