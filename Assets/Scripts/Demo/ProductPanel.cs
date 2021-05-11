using KitsuneCore.Services.Monetization.Products;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Demo
{
	public class ProductPanel : MonoBehaviour
	{
		[FormerlySerializedAs("iconContainer")] [SerializeField] private GameObject _iconContainer;
		[FormerlySerializedAs("nameTF")] [SerializeField] private TextMeshProUGUI _productNameTF;
		[FormerlySerializedAs("buyButton")] [SerializeField] private Button _buyButton;

		public Button BuyButton => _buyButton;
		
		private ProductEntity _product;
		public ProductEntity Product
		{
			get => _product;
			set
			{
				_product = value;
				Debug.Log("PRODUCT ID=" + _product.Id);
				_productNameTF.text = _product.Name;
				_buyButton.GetComponentInChildren<TextMeshProUGUI>().text = _product.DisplayPrice;
				Image iconObject = Resources.Load<Image>("Products/" + _product.Id);
				Instantiate(iconObject, _iconContainer.transform);
			}
		}
	}
}