using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KitsuneAPI.KitsuneEditor.Editor.Extensions;
using KitsuneAPI.KitsuneUnity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.CustomData
{
	[CustomEditor(typeof(KitsuneCustomData))]
	public class KitsuneCustomDataInspector : KitsuneBaseEntityInspector<KitsuneCustomData>
	{
		private string _jsonString;
		private JObject _jsonObject;
		private JProperty _property;
		private string _propertyName;
		private VisualElement _dataContainer;
		private List<string> _propertyTypes = new List<string>
		{
			"Empty Object",
			"Empty Array",
			"String",
			"Float",
			"Integer",
			"Boolean"
		};
		
		public override VisualElement CreateInspectorGUI()
		{
			CreateRootElement(false);
			
			_jsonString = serializedObject.FindProperty("_json").stringValue;
			
			// check validity if json is allowed to be edited directly
			try
			{
				if (!string.IsNullOrWhiteSpace(_jsonString))
				{
					_jsonObject = JsonConvert.DeserializeObject<JObject>(_jsonString);
				}
				else if (_jsonObject == null)
				{
					_jsonObject = new JObject();
				}
			}
			catch (JsonException e)
			{
				Debug.LogError("Error parsing Json=" + e.Message);
			}

			_dataContainer = _rootElement.Q<VisualElement>("DataContainer");
			
			AddRootDropDown(_rootElement.Q<VisualElement>("RootAddPropertyPopup"), _jsonObject);

			_dataContainer.Add(RecursiveRenderPropertyField(_jsonObject));

			return _rootElement;
		}

		protected override void OnSave()
		{
			serializedObject.UpdateIfRequiredOrScript();
			serializedObject.FindProperty("_json").stringValue = JsonConvert.SerializeObject(_jsonObject, Formatting.Indented);
			serializedObject.ApplyModifiedProperties();
			
			base.OnSave();
		}

		private void AddRootDropDown(VisualElement visualElement, JObject jObject)
		{
			VisualElement childContainer = new VisualElement();
			childContainer.AddToClassList("row");
			childContainer.AddToClassList("custom-data--root-add-custom-data-container");
			
			VisualElement popupContainer = new VisualElement();
			popupContainer.AddToClassList("custom-data--add-custom-data-popup-container");

			Button addButton = new Button();
			addButton.AddToClassList("custom-data--add-custom-data-button");
			
			childContainer.Add(popupContainer);
			childContainer.Add(addButton);
			
			visualElement.Add(childContainer);

			AddObjectPropertyDropDown(addButton, popupContainer, jObject);
		}

		private void AddPropertyWithDropDown(VisualElement parentElement, JToken jToken)
		{
			JProperty property = jToken.Value<JProperty>();

			VisualElement popupContainer = new VisualElement();
			popupContainer.AddToClassList("custom-data--add-custom-data-popup-container");
			parentElement.Add(popupContainer);
			
			Button addButton = new Button();
			addButton.AddToClassList("custom-data--add-custom-data-button");
			parentElement.Add(addButton);

			if (property.Value.Type == JTokenType.Object)
			{
				AddObjectPropertyDropDown(addButton, popupContainer, property.Value.Value<JObject>(), jToken);

			}
			else if (property.Value.Type == JTokenType.Array)
			{
				AddArrayPropertyDropdown(addButton, popupContainer, property.Value.Value<JArray>(), jToken);
			}
		}

		private void AddObjectPropertyDropDown(Button button, VisualElement parentElement, JObject jObject, JToken jToken = null)
		{
			List<string> propertyTypes = new List<string>(_propertyTypes);
			if (jToken != null)
			{
				propertyTypes.Add("Remove");
			}
			
			PopupField<string> dropdown = new PopupField<string>(propertyTypes, propertyTypes[0]);
			parentElement.Add(dropdown);

			button.clickable.clicked += () =>
			{
				switch (dropdown.value)
				{
					case "Empty Object":
						AddNewProperty<JObject>(jObject);
						break;
					case "Empty Array":
						AddNewProperty<JArray>(jObject);
						break;
					case "Float":
						AddNewProperty<float>(jObject);
						break;
					case "Integer":
						AddNewProperty<int>(jObject);
						break;
					case "Boolean":
						AddNewProperty<bool>(jObject);
						break;
					case "Remove":
						jToken.Remove();
						break;
					default:
						AddNewProperty<string>(jObject);
						break;
				}
				_dataContainer.Clear();
				_dataContainer.Add(RecursiveRenderPropertyField(_jsonObject));
			};
		}
		
		private void AddArrayPropertyDropdown(Button button, VisualElement parentElement, JArray jArray, JToken token = null)
		{
			List<string> propertyTypes = new List<string>(_propertyTypes);
			if (token != null)
			{
				propertyTypes.Add("Remove");
			}
			
			PopupField<string> dropdown = new PopupField<string>(propertyTypes, propertyTypes[0]);
			parentElement.Add(dropdown);

			button.clickable.clicked += () =>
			{
				switch (dropdown.value)
				{
					case "Empty Object":
						AddArrayValue<JObject>(jArray);
						break;
					case "Empty Array":
						AddArrayValue<JArray>(jArray);
						break;
					case "Float":
						AddArrayValue<float>(jArray);
						break;
					case "Integer":
						AddArrayValue<int>(jArray);
						break;
					case "Boolean":
						AddArrayValue<bool>(jArray);
						break;
					case "Remove":
						token.Remove();
						break;
					default:
						AddArrayValue<string>(jArray);
						break;
				}
				_dataContainer.Clear();
				_dataContainer.Add(RecursiveRenderPropertyField(_jsonObject));
			};
		}
		
		private void AddNewProperty<T>(JObject jObject)
		{
			serializedObject.UpdateIfRequiredOrScript();
			string typeName = typeof(T).Name.ToLower();
			object value = default(T);

			switch (Type.GetTypeCode(typeof(T)))
			{
				case TypeCode.Boolean:
					break;
				case TypeCode.Int32:
					typeName = "Integer";
					break;
				case TypeCode.Single:
					typeName = "Float";
					break;
				case TypeCode.String:
					value = "";
					break;
				default:
					if (typeof(T) == typeof(JObject))
					{
						typeName = "Empty Object";
						value = new JObject();
					}
					else if (typeof(T) == typeof(JArray))
					{
						typeName = "Empty Array";
						value = new JArray();
					}
					break;
			}

			string uniquePropertyName = EnsureUniquePropertyName(jObject, $"New {typeName}");
			JProperty property = new JProperty(uniquePropertyName, value);
			jObject.Add(property);
			serializedObject.ApplyModifiedProperties();
		}
		
		private void AddArrayValue<T>(JArray array)
		{
			object value = default(T);

			switch (Type.GetTypeCode(typeof(T)))
			{
				case TypeCode.Boolean:
					value = false;
					break;
				case TypeCode.Int32:
				case TypeCode.Single:
					value = 0;
					break;
				case TypeCode.String:
					value = "";
					break;
				default:
					if (typeof(T) == typeof(JObject))
					{
						value = new JObject();
					}
					else if (typeof(T) == typeof(JArray))
					{
						value = new JArray();
					}
					break;
			}

			array.Add(value);
		}
		
		private VisualElement RecursiveRenderPropertyField(JToken container, int indentLevel = 0)
	    {
		    VisualElement root = new VisualElement();
		    
		    for (int i = 0; i < container.Count(); ++i)
		    {
			    VisualElement propertyContainer = new VisualElement
			    {	
					style =
					{
						marginLeft = 5 + indentLevel * 10
					}
			    };
			    
			    root.Add(propertyContainer);

		        JToken token = container.Values<JToken>().ToArray()[i];
		        JProperty property = null;
		        VisualElement propValueContainer = new VisualElement();
		        propValueContainer.AddToClassList("row");
		        propValueContainer.AddToClassList("custom-data--property-value-container");
	            if (token.Type == JTokenType.Property)
	            {
		            property = token.Value<JProperty>();

		            string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(property.Name.ToLower()) + ":";
					TextField propertyLabelTF = new TextField();
					propertyLabelTF.AddToClassList("custom-data--property-label");
					propertyLabelTF.value = propertyName;
					propertyLabelTF.isDelayed = true;
					propertyLabelTF.RegisterValueChangedCallback(evt =>
					{
						string uniquePropertyName = EnsureUniquePropertyName((JObject)property.Parent, evt.newValue);
						property.Rename(uniquePropertyName);
					});
					
					propValueContainer.Add(propertyLabelTF);
					
					propertyContainer.Add(propValueContainer);
					
		            if (property.Value.Type == JTokenType.Object ||
		                property.Value.Type == JTokenType.Array)
		            {
			            AddPropertyWithDropDown(propValueContainer, token);
		            }
	            }
	            else if (token.Type == JTokenType.Object ||
	                     token.Type == JTokenType.Array)
	            {
		           propertyContainer.Add(RecursiveRenderPropertyField(token, indentLevel + 1));
	            }

	            if (property != null)
	            {
		            switch (property.Value.Type)
		            {
			            case JTokenType.Array:
				        case JTokenType.Object:
				            propertyContainer.Add(RecursiveRenderPropertyField(token, indentLevel + 1));
				            break;
			            case JTokenType.String:
				            string stringValue = (string)property.Value;
				            TextField propertyStringValue = new TextField();
				            propertyStringValue.multiline = true;
				            propertyStringValue.isDelayed = true;
				            propertyStringValue.AddToClassList("custom-data--property-value");
				            propertyStringValue.value = stringValue;
				            propValueContainer.Add(propertyStringValue);
				            propertyContainer.Add(propValueContainer);
				            propertyStringValue.RegisterValueChangedCallback(evt =>
				            {
					            property.Value = evt.newValue;
				            });
				          break;
			            case JTokenType.Float:
				            float floatValue = (float)property.Value;
				            FloatField propertyFloatValue = new FloatField();
				            propertyFloatValue.AddToClassList("custom-data--property-value");
				            propertyFloatValue.value = floatValue;
				            propertyFloatValue.isDelayed = true;
				            propValueContainer.Add(propertyFloatValue);
				            propertyContainer.Add(propValueContainer);
				            propertyFloatValue.RegisterValueChangedCallback(evt =>
				            {
					            property.Value = evt.newValue;
				            });
				            break;
			            case JTokenType.Integer:
				            int intValue = (int)property.Value;
				            IntegerField propertyIntValue = new IntegerField();
				            propertyIntValue.AddToClassList("custom-data--property-value");
				            propertyIntValue.value = intValue;
				            propertyIntValue.isDelayed = true;
				            propValueContainer.Add(propertyIntValue);
				            propertyContainer.Add(propValueContainer);
				            propertyIntValue.RegisterValueChangedCallback(evt =>
				            {
					            property.Value = evt.newValue;
				            });
				            break;
			            case JTokenType.Boolean:
				            bool boolValue = (bool)property.Value;
				            Toggle propertyBoolValue = new Toggle();
				            propertyBoolValue.AddToClassList("custom-data--property-value");
				            propertyBoolValue.value = boolValue;
				            propValueContainer.Add(propertyBoolValue);
				            propertyContainer.Add(propValueContainer);
				            propertyBoolValue.RegisterValueChangedCallback(evt =>
				            {
					            property.Value = evt.newValue;
				            });
				            break;
		            }
	            }
	        }

		    return root;
	    }

		private string EnsureUniquePropertyName(JObject jObject, string originalName)
		{
			string uniqueName = originalName;
			int index = 0;
			while (jObject[uniqueName] != null && index < 100)
			{
				index++;
				uniqueName = string.Format("{0} {1}", originalName, index.ToString());
			}
			return uniqueName;
		}
	}
}