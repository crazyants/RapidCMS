﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RapidCMS.Common.Data;
using RapidCMS.Common.Enums;
using RapidCMS.Common.Helpers;
using RapidCMS.Common.Interfaces;
using RapidCMS.Common.Models;
using RapidCMS.Common.Models.Config;
using RapidCMS.Common.Services;

namespace RapidCMS.Common.Extensions
{
    // TODO: make code more DRY

    public static class RapidCMSMiddleware
    {
        public static IServiceCollection AddRapidCMS(this IServiceCollection services, Action<Root> configure)
        {
            var root = new Root();

            configure.Invoke(root);

            services.AddSingleton(root);

            return services;
        }
    }

    public static class ICollectionRootExtensions
    {
        public static ICollectionRoot AddCollection<TEntity>(this ICollectionRoot root, string alias, string name, Action<CollectionConfig<TEntity>> configure)
            where TEntity : IEntity
        {
            var collection = new Collection
            {
                Name = name,
                Alias = alias
            };

            var configReceiver = new CollectionConfig<TEntity>();

            configure.Invoke(configReceiver);

            collection.RepositoryType = configReceiver.RepositoryType;

            if (configReceiver.TreeView != null)
            {
                collection.TreeView = new TreeView
                {
                    EntityViewType = configReceiver.TreeView.ViewType,
                    NameGetter = configReceiver.TreeView.PropertyMetadata.Getter
                };
            }

            collection.EntityVariants = configReceiver.EntityVariants.Any()
                ? configReceiver.EntityVariants.ToList(variant => new EntityVariant
                {
                    Icon = variant.Icon,
                    Name = variant.Name,
                    Type = variant.Type,
                    // TODO: GUID?
                    Alias = variant.Type.Name.ToUrlFriendlyString()
                })
                : new List<EntityVariant>
                {
                    new EntityVariant
                    {
                        // TODO: GUID?
                        Alias = typeof(TEntity).Name.ToUrlFriendlyString(),
                        Icon = null,
                        Name = typeof(TEntity).Name,
                        Type = typeof(TEntity)
                    }
                };

            if (configReceiver.ListView != null)
            {
                collection.ListView = new ListView
                {
                    Buttons = configReceiver.ListView.Buttons.ToList(button => button switch
                    {
                        DefaultButtonConfig defaultButton => defaultButton.ToDefaultButton(collection.EntityVariants),
                        CustomButtonConfig customButton => customButton.ToCustomButton(),
                        _ => default(Button)
                    }),
                    ViewPanes = configReceiver.ListView.ListViewPanes.ToList(pane =>
                    {
                        return new ViewPane<ListViewProperty>
                        {
                            Buttons = pane.Buttons.ToList(button => button switch
                            {
                                DefaultButtonConfig defaultButton => defaultButton.ToDefaultButton(collection.EntityVariants),
                                CustomButtonConfig customButton => customButton.ToCustomButton(),
                                _ => default(Button)
                            }),
                            Properties = pane.Properties.ToList(property => new ListViewProperty
                            {
                                Description = property.Description,
                                Name = property.Name,
                                NodeProperty = property.NodeProperty,
                                ValueMapper = property.ValueMapper ?? new DefaultValueMapper(),
                                ValueMapperType = property.ValueMapperType
                            })
                        };
                    })
                };
            }

            if (configReceiver.ListEditor != null)
            {
                var editors = configReceiver.ListEditor.ListEditors;

                collection.ListEditor = new ListEditor
                {
                    ListEditorType = configReceiver.ListEditor.ListEditorType,
                    Buttons = configReceiver.ListEditor.Buttons.ToList(button => button switch
                    {
                        DefaultButtonConfig defaultButton => defaultButton.ToDefaultButton(collection.EntityVariants),
                        CustomButtonConfig customButton => customButton.ToCustomButton(),
                        _ => default(Button)
                    }),
                    EditorPanes = editors.ToList(editor =>
                    {
                        return new EditorPane<Field>
                        {
                            VariantType = editor.VariantType,
                            Buttons = editor.Buttons.ToList(button => button switch
                            {
                                DefaultButtonConfig defaultButton => defaultButton.ToDefaultButton(collection.EntityVariants),
                                CustomButtonConfig customButton => customButton.ToCustomButton(),
                                _ => default(Button)
                            }),
                            Fields = editor.Fields.ToList(field =>
                            {
                                return new Field
                                {
                                    DataType = field.Type,
                                    Description = field.Description,
                                    Name = field.Name,
                                    NodeProperty = field.NodeProperty,
                                    Readonly = field.Readonly,
                                    ValueMapper = field.ValueMapper ?? new DefaultValueMapper(),
                                    ValueMapperType = field.ValueMapperType
                                };
                            })
                        };
                    })
                };
            }

            if (configReceiver.NodeEditor != null)
            {
                collection.NodeEditor = new NodeEditor
                {
                    Buttons = configReceiver.NodeEditor.Buttons.ToList(button => button switch
                    {
                        DefaultButtonConfig defaultButton => defaultButton.ToDefaultButton(collection.EntityVariants),
                        CustomButtonConfig customButton => customButton.ToCustomButton(),
                        _ => default(Button)
                    }),

                    BaseType = configReceiver.NodeEditor.BaseType,

                    EditorPanes = configReceiver.NodeEditor.EditorPanes.ToList(pane =>
                    {
                        return new EditorPane<Field>
                        {
                            VariantType = pane.VariantType,

                            Buttons = new List<Button>(),

                            Fields = pane.Fields.ToList(field =>
                            {
                                return new Field
                                {
                                    DataType = field.Type,
                                    Description = field.Description,
                                    Name = field.Name,
                                    NodeProperty = field.NodeProperty,
                                    Readonly = field.Readonly,
                                    ValueMapper = field.ValueMapper ?? new DefaultValueMapper(),
                                    ValueMapperType = field.ValueMapperType
                                };
                            }),

                            SubCollectionListEditors = pane.SubCollectionListEditors.ToList(listEditor =>
                            {
                                return new SubCollectionListEditor
                                {
                                    CollectionAlias = listEditor.CollectionAlias,

                                    // TODO: this is not good, the embedded view should get its own data
                                    // but then now it is a sub collection editor
                                    //Buttons = listEditor.Buttons.ToList(button => button switch
                                    //{
                                    //    DefaultButtonConfig defaultButton => defaultButton.ToDefaultButton(collection.EntityVariants),
                                    //    CustomButtonConfig customButton => customButton.ToCustomButton(),
                                    //    _ => default(Button)
                                    //}),
                                    //EditorPane = new EditorPane<Field>
                                    //{
                                    //    Buttons = listEditor.ListEditor.Buttons.ToList(button => button switch
                                    //    {
                                    //        DefaultButtonConfig defaultButton => new DefaultButton
                                    //        {
                                    //            ButtonId = Guid.NewGuid().ToString(),
                                    //            DefaultButtonType = defaultButton.ButtonType,
                                    //            Icon = defaultButton.Icon,
                                    //            Label = defaultButton.Label
                                    //        },
                                    //        _ => default(Button)
                                    //    }),
                                    //    Fields = listEditor.ListEditor.Fields.ToList(field =>
                                    //    {
                                    //        return new Field
                                    //        {
                                    //            DataType = field.Type,
                                    //            Description = field.Description,
                                    //            Name = field.Name,
                                    //            NodeProperty = field.NodeProperty,
                                    //            Readonly = field.Readonly,
                                    //            ValueMapper = field.ValueMapper ?? new DefaultValueMapper(),
                                    //            ValueMapperType = field.ValueMapperType
                                    //        };
                                    //    })
                                    //}
                                };
                            })
                        };
                    })
                };
            }

            collection.Collections = configReceiver.Collections;

            root.Collections.Add(collection);

            return root;
        }
    }

    public static class ButtonExtensions
    {
        public static IEnumerable<Button> GetAllButtons(this IEnumerable<Button> buttons)
        {
            // HACK: bit of a hack
            return buttons.SelectMany(x => x.Buttons.Any() ? x.Buttons.AsEnumerable() : new[] { x }).ToList();
        }
    }
}
