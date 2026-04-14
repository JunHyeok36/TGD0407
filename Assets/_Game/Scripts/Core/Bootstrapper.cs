namespace TDG0407.Core
{

    using Utils;
    using Systems.Data;

    /// <summary>
    /// 애플리케이션이 처음 시작할 때, 작동하는 클래스입니다.
    /// </summary>
    public sealed class Bootstrapper : Singleton<Bootstrapper>
    {
        protected override void Awake()
        {
            base.Awake();
            UserDataManager.Initialize();
        }
    }

}