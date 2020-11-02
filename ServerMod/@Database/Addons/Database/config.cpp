class CfgPatches {
    class Database {
        units[] = {};
        requiredAddons[] = {"DZ_Scripts"};
    };
};

class CfgMods {
    class Database {
        type = "mod";

        class defs {
            class gameScriptModule {
                value = "";
                files[] = {"Database/scripts/3_Game"};
            };
        };
    };
};