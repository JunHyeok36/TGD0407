namespace TDG0407.Systems.Managers
{

    using Core.Utils;
    using Domain.Archive;
    using Systems.Data;

    /// <summary>
    /// 애플리케이션이 처음 시작할 때, 작동하는 클래스입니다.
    /// </summary>
    public sealed class Bootstrapper : Singleton<Bootstrapper>
    {
        protected override void Awake()
        {
            base.Awake();

            InitializeManagers();
        }   

        private async void InitializeManagers()
        {
            await ArchiveManager.Initialize();
        }

    }

}