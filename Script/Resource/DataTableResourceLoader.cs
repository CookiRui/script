using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System;
using FixMath.NET;
using BW31.SP2D;

public class DataTableResourceLoader : ResourceLoader
{
    public static DataTableResourceLoader inst;

    void Awake()
    {
        inst = this;
    }
#if !SCENEEDITOR_TOOL
    public LuaTable getTblData(string tablename, int idx)
    {
        LuaTable tableData = LuaLoader.GetMainState().GetTable(tablename);

        LuaTable ret = tableData[idx] as LuaTable;

        return ret;
    }


    public T getTblData<T>(string tableName, int idx, string fieldName)
    {
        var raw = getTblData(tableName, idx, fieldName);

        T ret = castType<T>(raw);

        return ret;
    }

    T castType<T>(object obj)
    {
        Type destType = typeof(T);
        Type srcType = obj.GetType();

        object destObj = null;

        if (srcType == typeof(double))
        {
            if (destType == typeof(int))
            {
                int data = Convert.ToInt32(obj);
                destObj = data;
            }
            else if (destType == typeof(UInt32))
            {
                UInt32 data = Convert.ToUInt32(obj);
                destObj = data;
            }
            else if (destType == typeof(Fix64))
            {
                float data = Convert.ToSingle(obj);
                Fix64 d = (Fix64)data;
                destObj = d;
            }
            else if (destType == typeof(float))
            {
                float data = Convert.ToSingle(obj);
                destObj = data;
            }
            else if (destType == typeof(double))
            {
                double data = Convert.ToDouble(obj);
                destObj = data;
            }
        }
        else if (srcType == typeof(string))
        {
            if (destType == typeof(string))
            {
                string data = Convert.ToString(obj);
                destObj = data;
            }
        }
        else if (srcType == typeof(LuaTable))
        {
            LuaTable table = obj as LuaTable;
            //int tableLength = table.Length;
            /*if(destType == typeof(FixVector3[]))
            {
                int nVecNum = tableLength / 3;
                FixVector3[] de = new FixVector3[nVecNum];
                for (int i = 1; i <= tableLength; ++i)
                {
                    float cell = Convert.ToSingle(table[i]);
                    if(i%3==1)
                    {
                        de[i/3].x = (Fix64)cell;
                    }
                    else if (i % 3 == 2)
                    {
                        de[i/3].y = (Fix64)cell;
                    }
                    else if (i % 3 == 3)
                    {
                        de[i/3].z = (Fix64)cell;
                    }
                    else
                    {
                    }
                }
                destObj = de;
            }*/
            if (destType == typeof(FixVector2))
            {
                destObj = castFixVector2Data(table);
            }
            else if (destType == typeof(FixVector3))
            {
                destObj = castFixVector3Data(table);
            }
            else if (destType.IsGenericType && destType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                destObj = castMapData(table, destType);
            }
            //else if(destType.IsSubclassOf(typeof(System.Array)))
            else if (destType.IsArray)
            {
                destObj = castArrayData(table, destType);
            }
        }
        return (T)destObj;

    }

    protected object castTypeData(object source, Type destType)
    {
        if (destType == typeof(Fix64))
        {
            float cell = Convert.ToSingle(source);
            return (Fix64)cell;
        }
        else
        {
            return System.Convert.ChangeType(source, destType);
        }
    }

    protected FixVector2 castFixVector2Data(LuaTable table)
    {
        object[] list = table.ToArray();
        Type typeFix64 = typeof(Fix64);
        return new FixVector2((Fix64)castTypeData(list[0], typeFix64), (Fix64)castTypeData(list[1], typeFix64));
    }

    protected FixVector3 castFixVector3Data(LuaTable table)
    {
        object[] list = table.ToArray();
        Type typeFix64 = typeof(Fix64);
        return new FixVector3((Fix64)castTypeData(list[0], typeFix64), (Fix64)castTypeData(list[1], typeFix64), (Fix64)castTypeData(list[2], typeFix64));
    }

    protected object castArrayData(LuaTable table, Type destType)
    {
        Type arrayType = destType.GetElementType();
        object[] list = table.ToArray();
        int tableLength = list.Length;
        /*object[] ret = new object[tableLength];
        object temp;
        for (int index = 0; index < tableLength; index++)
        {
            if (arrayType == typeof(FixVector3))
            {
                LuaTable arraytable = list[index] as LuaTable;
                temp = castFixVector3Data(arraytable);
            }
            else if (arrayType.IsGenericType && arrayType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                LuaTable arraytable = list[index] as LuaTable;
                temp = castMapData(arraytable, arrayType);
            }
            else if (arrayType.IsArray)
            {
                LuaTable arraytable = list[index] as LuaTable;
                temp = castArrayData(arraytable, arrayType);
            }
            else temp = castTypeData(list[index], arrayType);
            ret[index] = temp;
        }*/
        object temp;
        Array retArray = Array.CreateInstance(arrayType, tableLength);
        for (int index = 0; index < tableLength; index++)
        {
            if (arrayType == typeof(FixVector2))
            {
                LuaTable arraytable = list[index] as LuaTable;
                temp = castFixVector2Data(arraytable);
            }
            else if (arrayType == typeof(FixVector3))
            {
                LuaTable arraytable = list[index] as LuaTable;
                temp = castFixVector3Data(arraytable);
            }
            else if (arrayType.IsGenericType && arrayType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                LuaTable arraytable = list[index] as LuaTable;
                temp = castMapData(arraytable, arrayType);
            }
            else if (arrayType.IsArray)
            {
                LuaTable arraytable = list[index] as LuaTable;
                temp = castArrayData(arraytable, arrayType);
            }
            else temp = castTypeData(list[index], arrayType);
            retArray.SetValue(temp, index);
        }
        return retArray;
    }

    protected object castMapData(LuaTable table, Type destType)
    {
        Type keyType = destType.GetGenericArguments()[0];
        Type valueType = destType.GetGenericArguments()[1];
        LuaDictTable dictTable = table.ToDictTable();

        //IDictionary ret = System.Activator.CreateInstance(typeof(Dictionary<object,object>)) as IDictionary;
        IDictionary ret = System.Activator.CreateInstance(destType) as IDictionary;
        object value;

        /*IEnumerator<DictionaryEntry> tableIter = dictTable.GetEnumerator();
        while (tableIter.MoveNext())
        {
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                LuaTable valueTable = tableIter.Current.Value as LuaTable;
                value = castMapData(valueTable, valueType);
            }
            else
            {
                value = castTypeData(tableIter.Current.Value, valueType);
            }
            ret.Add(castTypeData(tableIter.Current.Key, keyType), value);
        }*/

        IDictionaryEnumerator tableIter = dictTable.ToHashtable().GetEnumerator();
        while (tableIter.MoveNext())
        {
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                LuaTable valueTable = tableIter.Value as LuaTable;
                value = castMapData(valueTable, valueType);
            }
            else if (valueType.IsArray)
            {
                LuaTable valueTable = tableIter.Value as LuaTable;
                value = castArrayData(valueTable, valueType);
            }
            else if (valueType == typeof(FixVector2))
            {
                LuaTable valueTable = tableIter.Value as LuaTable;
                value = castFixVector2Data(valueTable);
            }
            else if (valueType == typeof(FixVector3))
            {
                LuaTable valueTable = tableIter.Value as LuaTable;
                value = castFixVector3Data(valueTable);
            }
            else
            {
                value = castTypeData(tableIter.Value, valueType);
            }
            //key只是基本类型 目前不扩展
            ret.Add(castTypeData(tableIter.Key, keyType), value);
            //ret[castTypeData(tableIter.Key, keyType)] = value;
        }
        return ret;
    }

    public object getTblData(string tablename, int idx, string fieldname)
    {
        LuaTable row = getTblData(tablename, idx);
        if (row == null)
            return null;
        object obj = row[fieldname];

        return obj;
    }
#endif
}
