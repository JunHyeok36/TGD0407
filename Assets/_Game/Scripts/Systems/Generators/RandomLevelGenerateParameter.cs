using System;

namespace TDG0407.Systems.Generators
{

    using Core.Value;
    using Domain.Map;
    
    public class RandomLevelGenerateParameter
    {
        #region Fields

        public string levelId;
        public int levelInstanceId;
        public LevelScale levelScale;
        
        public Random worldRandom;
        public Ref<int> nextEntityInstanceId;

        #endregion
        #region Constructors

        public RandomLevelGenerateParameter(
            string levelId, 
            int levelInstanceId, 
            LevelScale levelScale, 
            Random worldRandom, 
            Ref<int> nextEntityInstanceId)
        {
            this.levelId = levelId;
            this.levelInstanceId = levelInstanceId;
            this.levelScale = levelScale;
            this.worldRandom = worldRandom;
            this.nextEntityInstanceId = nextEntityInstanceId;
        }

        #endregion
    }

}