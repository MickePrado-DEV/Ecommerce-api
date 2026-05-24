namespace Ecommerce.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entity, object id) : base($"{entity} con {id} no encontrado")
        {

        }
    }
}
