
//#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IFunctionProxy<TCon>
    {
        T DoFunction<T>(IFunctionContainer<TCon, T> functionProxy);
    }

    public interface IFunctionContainer<TInCon, TOut>
    {
        TOut DoFunction<TIn>(TIn arg) where TIn : TInCon;
    }
}
