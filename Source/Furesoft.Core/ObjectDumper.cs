﻿using System.Collections;
using System.Reflection;
using System.Text;

namespace Furesoft.Core;

public sealed class ObjectDumper
{
    private readonly List<int> _hashListOfFoundElements;
    private readonly int _indentSize;
    private readonly StringBuilder _stringBuilder;
    private int _level;

    private ObjectDumper(int indentSize)
    {
        _indentSize = indentSize;
        _stringBuilder = new();
        _hashListOfFoundElements = [];
    }

    public static string Dump(object element)
    {
        return Dump(element, 2);
    }

    public static string Dump(object element, int indentSize)
    {
        var instance = new ObjectDumper(indentSize);
        return instance?.DumpElement(element);
    }

    private string DumpElement(object element)
    {
        if (element == null || element is ValueType || element is string)
        {
            Write(FormatValue(element));
        }
        else
        {
            var objectType = element?.GetType();
            if (!typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                Write("{{{0}}}", objectType?.FullName);
                _hashListOfFoundElements.Add(element.GetHashCode());
                _level++;
            }

            if (element is IEnumerable enumerableElement)
            {
                foreach (var item in enumerableElement)
                    if (item is IEnumerable && !(item is string))
                    {
                        _level++;
                        DumpElement(item);
                        _level--;
                    }
                    else
                    {
                        if (!AlreadyTouched(item))
                            DumpElement(item);
                        else
                            Write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName);
                    }
            }
            else
            {
                var members = element?.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                foreach (var memberInfo in members)
                {
                    var fieldInfo = memberInfo as FieldInfo;
                    var propertyInfo = memberInfo as PropertyInfo;

                    if (fieldInfo == null && propertyInfo == null)
                        continue;

                    var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo?.PropertyType;
                    var value = fieldInfo != null
                        ? fieldInfo.GetValue(element)
                        : propertyInfo?.GetValue(element, null);

                    if (type.IsValueType || type == typeof(string))
                    {
                        Write("{0}: {1}", memberInfo.Name, FormatValue(value));
                    }
                    else
                    {
                        var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                        Write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                        var alreadyTouched = !isEnumerable && AlreadyTouched(value);
                        _level++;
                        if (!alreadyTouched)
                            DumpElement(value);
                        else
                            Write("{{{0}}} <-- bidirectional reference found", value?.GetType().FullName);
                        _level--;
                    }
                }
            }

            if (!typeof(IEnumerable).IsAssignableFrom(objectType)) _level--;
        }

        return _stringBuilder?.ToString();
    }

    private bool AlreadyTouched(object value)
    {
        if (value == null)
            return false;

        var hash = value?.GetHashCode();
        for (var i = 0; i < _hashListOfFoundElements.Count; i++)
            if (_hashListOfFoundElements[i] == hash)
                return true;
        return false;
    }

    private void Write(string value, params object[] args)
    {
        var space = new string(' ', _level * _indentSize);

        if (args != null)
            value = string.Format(value, args);

        _stringBuilder.Append(space).AppendLine(value);
    }

    private string FormatValue(object o)
    {
        if (o == null)
            return "null";

        if (o is DateTime dateTime)
            return dateTime.ToShortDateString();

        if (o is string)
            return string.Format("\"{0}\"", o);

        if (o is char x && x == '\0')
            return string.Empty;

        if (o is ValueType)
            return o.ToString();

        if (o is IEnumerable)
            return "...";

        return "{ }";
    }
}