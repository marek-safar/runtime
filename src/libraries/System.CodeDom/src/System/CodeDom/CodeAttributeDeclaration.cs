// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.CodeDom
{
    public class CodeAttributeDeclaration
    {
        private string _name;
        private readonly CodeAttributeArgumentCollection _arguments = new CodeAttributeArgumentCollection();
        private CodeTypeReference _attributeType;

        public CodeAttributeDeclaration() { }

        public CodeAttributeDeclaration(string name)
        {
            Name = name;
        }

        public CodeAttributeDeclaration(string name, params CodeAttributeArgument[] arguments)
        {
            Name = name;
            Arguments.AddRange(arguments);
        }

        public CodeAttributeDeclaration(CodeTypeReference attributeType) : this(attributeType, null) { }

        public CodeAttributeDeclaration(CodeTypeReference attributeType, params CodeAttributeArgument[] arguments)
        {
            _attributeType = attributeType;
            if (attributeType is not null)
            {
                _name = attributeType.BaseType;
            }

            if (arguments is not null)
            {
                Arguments.AddRange(arguments);
            }
        }

        public string Name
        {
            get => _name ?? string.Empty;
            set
            {
                _name = value;
                _attributeType = new CodeTypeReference(_name);
            }
        }

        public CodeAttributeArgumentCollection Arguments => _arguments;

        public CodeTypeReference AttributeType => _attributeType;
    }
}
