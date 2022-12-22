namespace Conduit.Application.Interfaces;

public interface ISlugifier
{
    string Generate(string text);
}