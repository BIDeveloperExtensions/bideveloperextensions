namespace BIDSHelper.Core
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Custom property descriptor
    /// </summary>
    public class CustomPropertyDescriptor : PropertyDescriptor
    {
        /// <summary>
        /// Private field for property
        /// </summary>
        CustomProperty property;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPropertyDescriptor"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="attributes">The attributes.</param>
        public CustomPropertyDescriptor(ref CustomProperty property, Attribute[] attributes)
            : base(property.Name, attributes)
        {
            this.property = property;
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value.
        /// </summary>
        /// <param name="component">The component to test for reset capability.</param>
        /// <returns>
        /// true if resetting the component changes its value; otherwise, false.
        /// </returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, gets the type of the component this property is bound to.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of component this property is bound to. When the <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)"/> or <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)"/> methods are invoked, the object specified might be an instance of this type.
        /// </returns>
        public override Type ComponentType
        {
            get { return null; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the current value of the property on a component.
        /// </summary>
        /// <param name="component">The component with the property for which to retrieve the value.</param>
        /// <returns>
        /// The value of a property for a given component.
        /// </returns>
        public override object GetValue(object component)
        {
            return property.Value;
        }

        /// <summary>
        /// Gets the description of the member, as specified in the <see cref="T:System.ComponentModel.DescriptionAttribute"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The description of the member. If there is no <see cref="T:System.ComponentModel.DescriptionAttribute"/>, the property value is set to the default, which is an empty string ("").
        /// </returns>
        public override string Description
        {
            get { return property.Description; }
        }

        /// <summary>
        /// Gets the name of the category to which the member belongs, as specified in the <see cref="T:System.ComponentModel.CategoryAttribute"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The name of the category to which the member belongs. If there is no <see cref="T:System.ComponentModel.CategoryAttribute"/>, the category name is set to the default category, Misc.
        /// </returns>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
        /// </PermissionSet>
        public override string Category
        {
            get { return property.Category; }
        }

        /// <summary>
        /// Gets the name that can be displayed in a window, such as a Properties window.
        /// </summary>
        /// <value></value>
        /// <returns>The name to display for the member.</returns>
        public override string DisplayName
        {
            get { return property.Name; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether this property is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the property is read-only; otherwise, false.</returns>
        public override bool IsReadOnly
        {
            get { return property.ReadOnly; }
        }

        /// <summary>
        /// When overridden in a derived class, resets the value for this property of the component to the default value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be reset to the default value.</param>
        public override void ResetValue(object component)
        {
            // No action required
        }

        /// <summary>
        /// When overridden in a derived class, determines a value indicating whether the value of this property needs to be persisted.
        /// </summary>
        /// <param name="component">The component with the property to be examined for persistence.</param>
        /// <returns>
        /// true if the property should be persisted; otherwise, false.
        /// </returns>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, sets the value of the component to a different value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be set.</param>
        /// <param name="value">The new value.</param>
        public override void SetValue(object component, object value)
        {
            property.Value = value;
        }

        /// <summary>
        /// When overridden in a derived class, gets the type of the property.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the property.
        /// </returns>
        public override Type PropertyType
        {
            get { return property.Type; }
        }
    }
}
