using Sqids;

namespace MediatRApi.Helpers;

public static class AppHelpers
{
    public const string _Alphabet = "uAna3WTY4piy8EXq5IcNHmOFV2f9thLBgrCo6xwz7kGvQJlURbPMZSD10Ksedj";
    public const int _MinLength = 8;
    private static SqidsEncoder<int> GetSqids()
    {
        return  new SqidsEncoder<int>(new()
        {
            Alphabet = _Alphabet,
            MinLength = _MinLength
        });
    }

    public static string ToSqids(this int number){
        return GetSqids().Encode(number);
    }

    public static int FromSqids(this string encoded){
        return GetSqids().Decode(encoded).FirstOrDefault();
    }
}