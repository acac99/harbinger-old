module harbinger.Commands

open MediatR
open harbinger.CommandResultValue
open System.ComponentModel.DataAnnotations;

  
type CreateMessageCommand() =
    [<Required>]
    member this.text = ""
    interface IRequest<CommandResult>