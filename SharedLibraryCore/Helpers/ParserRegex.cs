﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraryCore.Interfaces
{
    public sealed class ParserRegex
    {
        /// <summary>
        /// represents the logical mapping of information provided by 
        /// game logs, get status, and get dvar information
        /// </summary>
        public enum GroupType
        {
            EventType,
            OriginNetworkId,
            TargetNetworkId,
            OriginClientNumber,
            TargetClientNumber,
            OriginName,
            TargetName,
            OriginTeam,
            TargetTeam,
            Weapon,
            Damage,
            MeansOfDeath,
            HitLocation,
            Message,
            RConClientNumber = 100,
            RConScore = 101,
            RConPing = 102,
            RConNetworkId = 103,
            RConName = 104,
            RConIpAddress = 105,
            RConDvarName = 106,
            RConDvarValue = 107,
            RConDvarDefaultValue = 108,
            RConDvarLatchedValue = 109,
            RConDvarDomain = 110,
            AdditionalGroup = 200
        }

        /// <summary>
        /// stores the regular expression groups that will be mapped to group types
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// stores the mapping from group type to group index in the regular expression
        /// </summary>
        public Dictionary<GroupType, int> GroupMapping { get; private set; }

        /// <summary>
        /// helper method to enable script parsers to app regex mapping
        /// the first parameter specifies the group type contained in the regex pattern
        /// the second parameter specifies the group index to retrieve in the matched regex pattern
        /// </summary>
        /// <param name="mapKey">group type</param>
        /// <param name="mapValue">group index</param>
        public void AddMapping(object mapKey, object mapValue)
        {
            if (int.TryParse(mapKey.ToString(), out int key) && int.TryParse(mapValue.ToString(), out int value))
            {
                if (GroupMapping.ContainsKey((GroupType)key))
                {
                    GroupMapping[(GroupType)key] = value;
                }

                else
                {
                    GroupMapping.Add((GroupType)key, value);
                }
            }

            if (mapKey.GetType() == typeof(GroupType) && mapValue.GetType().ToString() == "System.Int32")
            {
                GroupType k = (GroupType)Enum.Parse(typeof(GroupType), mapKey.ToString());
                int v = int.Parse(mapValue.ToString());

                if (GroupMapping.ContainsKey(k))
                {
                    GroupMapping[k] = v;
                }

                else
                {
                    GroupMapping.Add(k, v);
                }
            }
        }

        public ParserRegex()
        {
            GroupMapping = new Dictionary<GroupType, int>();
        }
    }
}
