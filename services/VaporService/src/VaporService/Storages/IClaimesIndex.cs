namespace VaporService.Storages
{
    public interface IClaimesIndex
    {
        bool ClaimWeapon(string userName, string weaponName);
        bool IsClaimed(string weaponName);
        bool IsOwner(string userName, string weaponName);
    }
}