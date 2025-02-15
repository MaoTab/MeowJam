using System;
using System.Collections.Generic;
using Godot;
using Yarn;
using Yarn.Compiler;

namespace YarnSpinnerGodot
{
    /// <summary>
    /// A declaration of a variable that is written to a yarn project
    /// </summary>
    [Tool]
    public class SerializedDeclaration
    {
        public static List<IType> BuiltInTypesList = new List<IType>
        {
            Types.String,
            Types.Boolean,
            Types.Number
        };

        public string name = "$variable";

        public string typeName = Types.String.Name;

        public bool defaultValueBool;
        public float defaultValueNumber;
        public string defaultValueString;

        public string description;

        public bool isImplicit;

        public string sourceYarnAssetPath;

        /// <summary>
        /// Set all of the serialized properties from a <see cref="Declaration"/> instance.
        /// </summary>
        /// <param name="decl"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetDeclaration(Declaration decl)
        {
            name = decl.Name;
            typeName = decl.Type.Name;
            description = decl.Description;
            isImplicit = decl.IsImplicit;
            sourceYarnAssetPath = ProjectSettings.LocalizePath(decl.SourceFileName);

            if (typeName == Types.String.Name)
            {
                defaultValueString = Convert.ToString(decl.DefaultValue);
            }
            else if (typeName == Types.Boolean.Name)
            {
                defaultValueBool = Convert.ToBoolean(decl.DefaultValue);
            }
            else if (typeName == Types.Number.Name)
            {
                defaultValueNumber = Convert.ToSingle(decl.DefaultValue);
            }
            else
            {
                throw new InvalidOperationException($"Invalid declaration type {decl.Type.Name}");
            }
        }
    }
}