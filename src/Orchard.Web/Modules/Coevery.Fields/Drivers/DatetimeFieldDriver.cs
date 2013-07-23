﻿using System;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Coevery.Fields.Fields;
using Coevery.Fields.Settings;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Coevery.Fields.Drivers {
    public class DatetimeFieldDriver : ContentFieldDriver<DatetimeField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Datetime.Edit";

        public DatetimeFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
            DisplayName = "Datetime";
            Description = "Allows users to enter date and time.";
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(DatetimeField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, DatetimeField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_Datetime", GetDifferentiator(field, part), () => {
                var settings = field.PartFieldDefinition.Settings.GetModel<DatetimeFieldSettings>();
                return shapeHelper.Fields_Input().Settings(settings);
            });
        }

        protected override DriverResult Editor(ContentPart part, DatetimeField field, dynamic shapeHelper) {
            return ContentShape("Fields_Datetime_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: field, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, DatetimeField field, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(field, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<DatetimeFieldSettings>();

                if (settings.Required && !field.Value.HasValue) {
                    updater.AddModelError(GetPrefix(field, part), T("The field {0} is mandatory.", T(field.DisplayName)));
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, DatetimeField field, ImportContentContext context) {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Value", v => field.Value = DateTime.Parse(v));
        }

        protected override void Exporting(ContentPart part, DatetimeField field, ExportContentContext context) {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Value", field.Value);
        }

        protected override void Describe(DescribeMembersContext context) {
            context.Member(null, typeof(DateTime?), T("Value"), T("The datetime value of the field."));
        }
    }
}
