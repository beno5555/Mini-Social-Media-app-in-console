namespace social_media_console_app.BusinessLogic.Mappers.Base;

public interface IMapper<TEntity, in TCreate, out TDisplay>
{
    public TEntity  ToEntity(TCreate displayDto);
    public TDisplay ToDisplay(TEntity entity);
}