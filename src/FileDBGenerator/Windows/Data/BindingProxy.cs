using System.Windows;

namespace FileDBGenerator.Windows.Data
{
    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            name: "Data",
            propertyType: typeof(object),
            ownerType: typeof(BindingProxy),
            typeMetadata: new UIPropertyMetadata(null)
        );
    }

}
