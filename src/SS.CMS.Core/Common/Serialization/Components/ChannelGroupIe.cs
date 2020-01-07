﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SS.CMS.Models;
using SS.CMS.Repositories;
using SS.CMS.Utils;
using SS.CMS.Utils.Atom.Atom.Core;

namespace SS.CMS.Core.Serialization.Components
{
    public static class ChannelGroupIe
    {
        public static AtomEntry Export(ChannelGroup groupInfo)
        {
            var entry = AtomUtility.GetEmptyEntry();

            AtomUtility.AddDcElement(entry.AdditionalElements, "IsNodeGroup", true.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, new List<string> { nameof(ChannelGroup.GroupName), "NodeGroupName" }, groupInfo.GroupName);
            AtomUtility.AddDcElement(entry.AdditionalElements, nameof(ChannelGroup.Taxis), groupInfo.Taxis.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, nameof(ChannelGroup.Description), groupInfo.Description);

            return entry;
        }

        public static async Task<bool> ImportAsync(AtomEntry entry, int siteId, IChannelGroupRepository channelGroupRepository)
        {
            var isNodeGroup = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, "IsNodeGroup"));
            if (!isNodeGroup) return false;

            var groupName = AtomUtility.GetDcElementContent(entry.AdditionalElements, new List<string> { nameof(ChannelGroup.GroupName), "NodeGroupName" });
            if (string.IsNullOrEmpty(groupName)) return true;
            if (await channelGroupRepository.IsExistsAsync(siteId, groupName)) return true;

            var taxis = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, nameof(ChannelGroup.Taxis)));
            var description = AtomUtility.GetDcElementContent(entry.AdditionalElements, nameof(ChannelGroup.Description));
            await channelGroupRepository.InsertAsync(new ChannelGroup
            {
                GroupName = groupName,
                SiteId = siteId,
                Taxis = taxis,
                Description = description
            });

            return true;
        }
    }
}
