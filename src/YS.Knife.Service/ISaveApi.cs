namespace YS.Knife.Service
{
    public interface ISaveApi<TSaveDto>
    {
        Task Save(TSaveDto[] Dtos, CancellationToken token = default);
    }

   
}
