using System.Collections;
using System.DirectoryServices.Protocols;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Infra.Tests.Features.BackgroundWorker.Helpers;

/// <summary>
/// SearchResponse и SearchResultEntry — sealed классы без публичных конструкторов.
/// Используем RuntimeHelpers.GetUninitializedObject + reflection по полям.
/// </summary>
internal static class LdapTestHelpers
{
    private static readonly BindingFlags NonPublic =
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

    public static SearchResponse CreateSearchResponse(params SearchResultEntry[] entries)
    {
        var response = (SearchResponse)RuntimeHelpers
            .GetUninitializedObject(typeof(SearchResponse));

        var collection = (SearchResultEntryCollection)RuntimeHelpers
            .GetUninitializedObject(typeof(SearchResultEntryCollection));

        var innerList = new ArrayList();
        foreach (var entry in entries)
            innerList.Add(entry);

        // SearchResultEntryCollection хранит записи в ArrayList
        SetField(collection, "_list", innerList);

        // SearchResponse хранит коллекцию в поле, а не свойстве
        SetField(response, "_entryCollection", collection);

        return response;
    }

    public static SearchResultEntry CreateEntry(
        string dn,
        params (string name, string value)[] attributes)
    {
        var entry = (SearchResultEntry)RuntimeHelpers
            .GetUninitializedObject(typeof(SearchResultEntry));

        // DN хранится в backing field автосвойства
        SetField(entry, "<DistinguishedName>k__BackingField", dn);

        var attrCollection = (SearchResultAttributeCollection)RuntimeHelpers
            .GetUninitializedObject(typeof(SearchResultAttributeCollection));

        var dict = new Hashtable(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, value) in attributes)
        {
            var attr = new DirectoryAttribute();
            attr.Name = name;
            attr.Add(value);
            dict[name] = attr;
        }

        SetField(attrCollection, "_hashtable", dict);
        SetField(entry, "<Attributes>k__BackingField", attrCollection);

        return entry;
    }

    private static void SetField(object obj, string fieldName, object? value)
    {
        var type = obj.GetType();
        FieldInfo? field = null;

        // Ищем поле по иерархии типов
        while (type != null && field == null)
        {
            field = type.GetField(fieldName, NonPublic);
            type = type.BaseType;
        }

        if (field is null)
        {
            throw new InvalidOperationException(
                $"Field '{fieldName}' not found on {obj.GetType().FullName}. " +
                $"Available fields: {string.Join(", ", GetAllFields(obj.GetType()))}");
        }

        field.SetValue(obj, value);
    }

    private static IEnumerable<string> GetAllFields(Type type)
    {
        var fields = new List<string>();
        var t = type;
        while (t != null)
        {
            fields.AddRange(t.GetFields(NonPublic).Select(f => f.Name));
            t = t.BaseType;
        }

        return fields;
    }
}