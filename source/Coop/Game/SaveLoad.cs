﻿using Coop.Game.Patch;
using SandBox;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;
using TaleWorlds.SaveSystem.Save;

namespace Coop.Game
{
    public static class SaveLoad
    {
        public static SaveOutput SaveGame(TaleWorlds.Core.Game game, ISaveDriver driver)
        {
            var entitySystem = Utils.GetPrivateField<EntitySystem<GameHandler>>(typeof(TaleWorlds.Core.Game), "_gameEntitySystem", game);
            MetaData metaData = GetMetaData();

            // Code copied from TaleWorlds.Game.Save(MetaData, ISaveDriver)
            foreach (GameHandler gameHandler in entitySystem.Components)
            {
                gameHandler.OnBeforeSave();
            }
            SaveOutput saveOutput = SaveManager.Save(game, metaData, driver);
            saveOutput.PrintStatus();
            foreach (GameHandler gameHandler2 in entitySystem.Components)
            {
                gameHandler2.OnAfterSave();
            }
            // End code copy

            return saveOutput;
        }
        public static void LoadGame(LoadResult loadResult)
        {
            if(TaleWorlds.Core.Game.Current != null)
            {
                GameStateManager.Current.CleanStates(0);
                GameStateManager.Current = Module.CurrentModule.GlobalGameStateManager;
            }
            MBGameManager.StartNewGame(new CampaignGameManager(loadResult));
        }
        public static LoadGameResult LoadSaveGameData(ISaveDriver driver)
        {
            var currentModules = GetModules();
            LoadResult loadResult = SaveManager.Load(driver);
            if (loadResult.Successful)
            {
                return new LoadGameResult(loadResult, CheckModules(driver.LoadMetaData(), currentModules));
            }
            return null;
        }
        public static List<ModuleCheckResult> CheckModules(MetaData fileMetaData, List<ModuleInfo> loadedModules)
        {
            return Utils.InvokePrivateMethod<List<ModuleCheckResult>>(typeof(MBSaveLoad), "CheckModules", null, new object[] { fileMetaData, loadedModules });
        }
        public static List<ModuleInfo> GetModules()
        {
            List<ModuleInfo> list = new List<ModuleInfo>();
            foreach (var moduleName in Utilities.GetModulesNames())
            {
                ModuleInfo moduleInfo = new ModuleInfo();
                moduleInfo.Load(moduleName);
                list.Add(moduleInfo);
            }
            return list;
        }
        public static MetaData GetMetaData()
        {
            CampaignSaveMetaDataArgs args = Utils.InvokePrivateMethod<CampaignSaveMetaDataArgs>(typeof(SaveHandler), "GetSaveMetaData", Campaign.Current.SaveHandler);
            return Utils.InvokePrivateMethod<MetaData>(typeof(MBSaveLoad), "GetSaveMetaData", null, new object[] { args });
        }
    }
}