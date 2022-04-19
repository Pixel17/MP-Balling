﻿using GmmlPatcher;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using GmmlHooker;
using TSIMPH;

namespace WYSMultiplayer
{
    public class GameMakerMod : IGameMakerMod
    {
        // from snailax source
        public static Dictionary<string, string> GMLkvp = new Dictionary<string, string>();

        public static GlobalDecompileContext? GDC;

        public static bool LoadGMLFolder(string gmlfolder)
        {
            return GMLkvp.LoadGMLFolder(gmlfolder);
        }

        public static UndertaleScript CreateScriptFromKVP(UndertaleData data, string name, string key, ushort arguments)
        {
            return data.CreateLegacyScript(name, GMLkvp[key], arguments);
        }

        // my code
        public void Load(int audioGroup, UndertaleData data, ModData currentMod)
        {
            GDC = new GlobalDecompileContext(data, false);
            //supress vs being stupid (i mean he's not wrong)
            string gmlfolder = Path.Combine(currentMod.path, "GMLSource");

            LoadGMLFolder(gmlfolder);

            UndertaleGameObject multiplayermanager_obj = new UndertaleGameObject();

            multiplayermanager_obj.Name = data.Strings.MakeString("obj_multiplayer_manager");

            multiplayermanager_obj.EventHandlerFor(EventType.Step, EventSubtypeStep.Step, data.Strings, data.Code, data.CodeLocals)
                .AppendGmlSafe(GMLkvp["gml_obj_multiplayer_manager_Step_0"], data);
            multiplayermanager_obj.EventHandlerFor(EventType.Other, EventSubtypeOther.AsyncNetworking, data.Strings, data.Code, data.CodeLocals)
                .AppendGmlSafe(GMLkvp["gml_obj_multiplayer_manager_Network"], data);

            data.GameObjects.Add(multiplayermanager_obj);

            CreateScriptFromKVP(data, "scr_send_position", "gml_Script_scr_send_position", 2);

            CreateScriptFromKVP(data, "scr_send_player_info", "gml_Script_scr_send_player_info", 5);

            // gml_RoomCC_room_multiplayer_Create
            data.CreateCode("gml_RoomCC_room_multiplayer_Create", GMLkvp["gml_RoomCC_room_multiplayer_Create"]);

            try
            {

                data.Code.First(code => code.Name.Content == "gml_Object_obj_epilepsy_warning_Create_0")
                    .AppendGML("txt_1 = \"WORKS\"\ntxt_2 = \"The fitness gram pacer test is \na multi stage arobic capacity test.", data);
            }
            // UndertaleModLib is trying to write profile cache but fails, we don't care (i dont care more)
            catch (Exception) { /* ignored */ }

            UndertaleGameObject mp_player_obj = new UndertaleGameObject();

            data.GameObjects.Add(mp_player_obj);

            mp_player_obj.Name = data.Strings.MakeString("obj_mp_player");

            mp_player_obj.Sprite = data.Sprites.ByName("spr_player");

            mp_player_obj.EventHandlerFor(EventType.Draw, EventSubtypeDraw.Draw, data.Strings, data.Code, data.CodeLocals)
                .AppendGmlSafe(GMLkvp["gml_Object_obj_mp_player_Draw_0"], data);

            UndertaleRoom mp_room = Conviences.CreateBlankLevelRoom("room_multiplayer", data);

            mp_room.SetupRoom(false);

            mp_room.AddObjectToLayer(data, "obj_multiplayer_manager", "Player");

            mp_room.AddObjectToLayer(data, "obj_player", "Player");

            mp_room.CreationCodeId = data.Code.ByName("gml_RoomCC_room_multiplayer_Create");

            mp_room.SetupRoom(false);


            data.Rooms.Add(mp_room);

            try
            {

                data.Code.First(code => code.Name.Content == "gml_Object_obj_player_Step_0")
                    .AppendGML("if keyboard_check_pressed(vk_alt)\nscr_fade_to_room(room_multiplayer)", data);
            }
            // UndertaleModLib is trying to write profile cache but fails, we don't care
            catch (Exception) { /* ignored */ }
        }
    }
}