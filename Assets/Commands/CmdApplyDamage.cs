public class CmdApplyDamage : ICommand
{
    private IDamageable _damageable;
    private int _damage;
    private DamageType _type;

    public CmdApplyDamage(IDamageable damageable, int damage, DamageType type)
    {
        _damageable = damageable;
        _damage = damage;
        _type = type;
    }

    public void Execute() => _damageable.ApplyDamage(_damage, _type);
}
