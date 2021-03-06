﻿using System;
using System.Linq;
using System.Collections.Generic;
using Coevery.ContentManagement.MetaData;
using Coevery.ContentManagement.MetaData.Models;
using Coevery.Core.Contents.Settings;
using Coevery.Environment.Extensions.Models;
using Coevery.Security.Permissions;

namespace Coevery.Core.Contents {
    public class DynamicPermissions : IPermissionProvider {
        private static readonly Permission PublishContent = new Permission { Description = "Publish or unpublish {0} for others", Name = "Publish_{0}", ImpliedBy = new[] { Permissions.PublishContent } };
        private static readonly Permission PublishOwnContent = new Permission { Description = "Publish or unpublish {0}", Name = "PublishOwn_{0}", ImpliedBy = new[] { PublishContent, Permissions.PublishOwnContent } };
        private static readonly Permission EditContent = new Permission { Description = "Edit {0} for others", Name = "Edit_{0}", ImpliedBy = new[] { PublishContent, Permissions.EditContent } };
        private static readonly Permission EditOwnContent = new Permission { Description = "Edit {0}", Name = "EditOwn_{0}", ImpliedBy = new[] { EditContent, PublishOwnContent, Permissions.EditOwnContent } };
        private static readonly Permission DeleteContent = new Permission { Description = "Delete {0} for others", Name = "Delete_{0}", ImpliedBy = new[] { Permissions.DeleteContent } };
        private static readonly Permission DeleteOwnContent = new Permission { Description = "Delete {0}", Name = "DeleteOwn_{0}", ImpliedBy = new[] { DeleteContent, Permissions.DeleteOwnContent } };
        private static readonly Permission ViewContent = new Permission { Description = "View {0} by others", Name = "View_{0}", ImpliedBy = new[] { EditContent, Permissions.ViewContent } };
        private static readonly Permission ViewOwnContent = new Permission { Description = "View own {0}", Name = "ViewOwn_{0}", ImpliedBy = new[] { ViewContent, Permissions.ViewOwnContent } };

        public static readonly Dictionary<string, Permission> PermissionTemplates = new Dictionary<string, Permission> {
            {Permissions.PublishContent.Name, PublishContent},
            {Permissions.PublishOwnContent.Name, PublishOwnContent},
            {Permissions.EditContent.Name, EditContent},
            {Permissions.EditOwnContent.Name, EditOwnContent},
            {Permissions.DeleteContent.Name, DeleteContent},
            {Permissions.DeleteOwnContent.Name, DeleteOwnContent},
            {Permissions.ViewContent.Name, ViewContent},
            {Permissions.ViewOwnContent.Name, ViewOwnContent}
        };

        private readonly IContentDefinitionManager _contentDefinitionManager;

        public virtual Feature Feature { get; set; }

        public DynamicPermissions(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public IEnumerable<Permission> GetPermissions() {
            // manage rights only for Creatable types
            var creatableTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable);

            foreach (var typeDefinition in creatableTypes) {
                foreach (var permissionTemplate in PermissionTemplates.Values) {
                    yield return CreateDynamicPermission(permissionTemplate, typeDefinition);
                }
            }
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

        /// <summary>
        /// Returns a dynamic permission for a content type, based on a global content permission template
        /// </summary>
        public static Permission ConvertToDynamicPermission(Permission permission) {
            if (PermissionTemplates.ContainsKey(permission.Name)) {
                return PermissionTemplates[permission.Name];
            }

            return null;
        }

        /// <summary>
        /// Generates a permission dynamically for a content type
        /// </summary>
        public static Permission CreateDynamicPermission(Permission template, ContentTypeDefinition typeDefinition) {
            return new Permission {
                Name = String.Format(template.Name, typeDefinition.Name),
                Description = String.Format(template.Description, typeDefinition.DisplayName),
                Category = typeDefinition.DisplayName,
                ImpliedBy = (template.ImpliedBy ?? new Permission[0]).Select(t => CreateDynamicPermission(t, typeDefinition))
            };
        }
    }
}
