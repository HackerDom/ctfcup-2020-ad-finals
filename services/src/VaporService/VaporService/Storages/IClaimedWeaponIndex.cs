namespace VaporService.Storages
{
    public interface IClaimedWeaponIndex
    {
        bool ClaimWeapon(string userName, string weaponName);
        bool IsClaimed(string weaponName);
        bool IsOwner(string userName, string weaponName);
    }
}