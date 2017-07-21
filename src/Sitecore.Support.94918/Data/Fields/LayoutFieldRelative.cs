namespace Sitecore.Support.Data.Fields
{
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Layouts;
    using Sitecore.Links;
    using Sitecore.Text;
    using System.Collections;

    public class LayoutFieldRelative : LayoutField
    {
        public LayoutFieldRelative(Item item)
          : base(item.Fields[FieldIDs.LayoutField])
        {
        }

        public LayoutFieldRelative(Field innerField)
          : base(innerField)
        {
        }

        public LayoutFieldRelative(Field innerField, string runtimeValue)
          : base(innerField, runtimeValue)
        {
        }

        /// <summary>Validates the links.</summary>
        /// <param name="result">The result.</param>
        public override void ValidateLinks(LinksValidationResult result)
        {
            Assert.ArgumentNotNull((object)result, "result");
            string xml = this.Value;
            if (string.IsNullOrEmpty(xml))
                return;
            ArrayList devices = LayoutDefinition.Parse(xml).Devices;
            if (devices == null)
                return;
            foreach (DeviceDefinition device in devices)
            {
                if (!string.IsNullOrEmpty(device.ID))
                {
                    Item targetItem = this.InnerField.Database.GetItem(device.ID);
                    if (targetItem != null)
                        result.AddValidLink(targetItem, device.ID);
                    else
                        result.AddBrokenLink(device.ID);
                }
                if (!string.IsNullOrEmpty(device.Layout))
                {
                    Item targetItem = this.InnerField.Database.GetItem(device.Layout);
                    if (targetItem != null)
                        result.AddValidLink(targetItem, device.Layout);
                    else
                        result.AddBrokenLink(device.Layout);
                }
                this.ValidatePlaceholderSettings(result, device);
                if (device.Renderings != null)
                {
                    foreach (RenderingDefinition rendering in device.Renderings)
                    {
                        if (rendering.ItemID != null)
                        {
                            Item obj = this.InnerField.Database.GetItem(rendering.ItemID);
                            if (obj != null)
                                result.AddValidLink(obj, rendering.ItemID);
                            else
                                result.AddBrokenLink(rendering.ItemID);
                            if (!string.IsNullOrEmpty(rendering.Datasource))
                            {
                                string path = this.InnerField.Item.Paths.FullPath + "/" + rendering.Datasource;
                                if (this.InnerField.Database.GetItem(rendering.Datasource) == null && this.InnerField.Database.GetItem(path) != null)
                                    rendering.Datasource = path;
                                Item targetItem = this.InnerField.Database.GetItem(rendering.Datasource);
                                if (targetItem != null)
                                    result.AddValidLink(targetItem, rendering.Datasource);
                                else if (!rendering.Datasource.Contains(":"))
                                    result.AddBrokenLink(rendering.Datasource);
                            }
                            if (obj != null && !string.IsNullOrEmpty(rendering.Parameters))
                            {
                                foreach (CustomField customField in this.GetParametersFields(obj, rendering.Parameters).Values)
                                    customField.ValidateLinks(result);
                            }
                        }
                    }
                }
            }
        }
        private RenderingParametersFieldCollection GetParametersFields(Item layoutItem, string renderingParameters)
        {
            UrlString parameters = new UrlString(renderingParameters);
            RenderingParametersFieldCollection parametersFields;
            RenderingParametersFieldCollection.TryParse(layoutItem, parameters, out parametersFields);
            return parametersFields;
        }
    }
}