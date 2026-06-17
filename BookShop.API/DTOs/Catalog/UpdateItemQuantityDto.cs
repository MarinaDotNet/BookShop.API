namespace BookShop.API.DTOs.Catalog;

/// <summary>
/// Represents the data required to update the quantity of an item in the shopping cart.
/// </summary>
/// <param name="Quantity">
/// The new quantity. Must be greater than zero.
/// </param>
public sealed record UpdateItemQuantityDto(int Quantity);