﻿using System;
using RapidCMS.Common.Data;
using RapidCMS.Common.Enums;
using RapidCMS.Common.Models.Metadata;
using RapidCMS.Common.ValueMappers;

namespace RapidCMS.Common.Models.UI
{
    public class FieldUI : ElementUI
    {
        public string Name { get; internal set; }
        public string Description { get; internal set; }

        public EditorType Type { get; internal set; }
    }

    public class ExpressionFieldUI : FieldUI
    {
        public IExpressionMetadata Expression { get; internal set; }
    }

    public class PropertyFieldUI : FieldUI
    {
        public IValueMapper ValueMapper { get; internal set; }
        public IPropertyMetadata Property { get; internal set; }
        public IDataCollection DataCollection { get; internal set; }
    }

    public class CustomPropertyFieldUI : PropertyFieldUI
    {
        public string CustomAlias { get; internal set; }
    }
}
