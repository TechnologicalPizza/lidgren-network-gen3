﻿/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Reflection;

namespace Lidgren.Network
{
    public partial class NetBuffer
    {
        /// <summary>
        /// Writes all public and private declared instance fields of the object in alphabetical order using reflection.
        /// </summary>
        public void WriteAllFields(object ob)
        {
            WriteAllFields(
                ob,
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Writes all fields with specified binding in alphabetical order using reflection.
        /// </summary>
        public void WriteAllFields(object ob, BindingFlags flags)
        {
            if (ob == null)
                return;

            Type tp = ob.GetType();
            FieldInfo[] fields = tp.GetFields(flags);
            NetUtility.SortMembersList(fields);

            foreach (FieldInfo fi in fields)
            {
                var value = fi.GetValue(ob);

                // find the appropriate Write method
                if (WriteMethods.TryGetValue(fi.FieldType, out var writeMethod))
                    writeMethod.Invoke(this, new[] { value });
                else
                    throw new LidgrenException("Failed to find write method for type " + fi.FieldType);
            }
        }

        /// <summary>
        /// Writes all public and private declared instance properties of the object in alphabetical order using reflection.
        /// </summary>
        public void WriteAllProperties(object ob)
        {
            WriteAllProperties(
                ob,
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Writes all properties with specified binding in alphabetical order using reflection.
        /// </summary>
        public void WriteAllProperties(object ob, BindingFlags flags)
        {
            if (ob == null)
                return;

            Type type = ob.GetType();
            PropertyInfo[] fields = type.GetProperties(flags);
            NetUtility.SortMembersList(fields);

            foreach (PropertyInfo fi in fields)
            {
                var getMethod = fi.GetMethod;
                if (getMethod == null)
                    continue;
                {
                    var value = getMethod.Invoke(ob, null);

                    // find the appropriate Write method
                    if (WriteMethods.TryGetValue(fi.PropertyType, out var writeMethod))
                        writeMethod.Invoke(this, new[] { value });
                    else
                        throw new LidgrenException("Failed to find write method for type " + fi.PropertyType);
                }
            }
        }
    }
}