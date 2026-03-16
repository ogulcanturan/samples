using System.ServiceModel;

namespace Wcf.Abstractions
{
    [ServiceContract]
    public interface ISimpleWcfService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);
    }
}