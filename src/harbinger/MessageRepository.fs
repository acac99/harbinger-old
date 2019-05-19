module harbinger.MessageRepository
open Microsoft.EntityFrameworkCore
open harbinger.MessageDto

type HarbingerContext (options: DbContextOptions<HarbingerContext>) =
    inherit DbContext(options)
    
    override __.OnModelCreating modelBuilder =
        modelBuilder.Entity<Message>().Property(fun x -> x.Id).HasDefaultValueSql("NEWID()") |> ignore
        
    override __.OnConfiguring(optionsBuilder) =
        optionsBuilder
            .UseNpgsql("Host=localhost;Database=harbinger;Username=postgres;Password=postgres")
            |> ignore
            
    [<DefaultValue>]
    val mutable message:DbSet<Message>
    member x.messages 
        with get() = x.message 
        and set v = x.message <- v
    

type IMessageRepository =
    abstract Create : Message -> Async<Message option>
    
type MessageRepository(HarbingerContext: HarbingerContext) =
    member this.HarbingerContext = HarbingerContext
    interface IMessageRepository with
        member this.Create(message) =
            async {
                this.HarbingerContext.messages.AddAsync(message)
                |> Async.AwaitTask
                |> ignore
                
                let! result = this.HarbingerContext.SaveChangesAsync true |> Async.AwaitTask
                let result = if result >= 1  then Some(message) else None
                return result
            }
            