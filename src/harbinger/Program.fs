module harbinger.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Http
open harbinger.MessageDto
open harbinger.MessageRepository
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe.HttpStatusCodeHandlers
open MediatR;
open harbinger.Commands



let messageAddHandler : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
              let mediator = ctx.RequestServices.GetService(typeof<IMediator>) :?> IMediator
              let! command = ctx.BindJsonAsync<CreateMessageCommand>()
              mediator.Send(command) |> Async.AwaitTask |> ignore
              return! ctx.WriteJsonAsync "Woo"
              
//            let messageRepository = ctx.RequestServices.GetService(typeof<IMessageRepository>) :?> IMessageRepository
//            let! messageRequest = ctx.BindJsonAsync<MessageRequest>()
//            let! createdMessage = messageRepository.Create(messageRequest.GetMessage)
//            return! ctx.WriteJsonAsync createdMessage
        }
      



let webApp =
    choose [
        GET >=> 
            choose [
                route "/message" >=> text "test"
            ]
        
        POST >=>
            choose [
                route "/message" >=> messageAddHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------


let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddTransient<IMessageRepository, MessageRepository>() |> ignore
    services.AddEntityFrameworkNpgsql()
        .AddDbContext<HarbingerContext>()
        .BuildServiceProvider() |> ignore
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddMediatR(typeof<CreateMessageCommand>) |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0