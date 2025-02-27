using Rock.Communication.Chat;
using Rock.Model;

using System.Collections.Generic;
using Rock.SystemGuid;

using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication.Chat
{
    [DisplayName( "Chat" )]
    [Category( "Chat" )]
    [Description( "" )]
    [SupportedSiteTypes( SiteType.Mobile )]

    [Rock.SystemGuid.EntityTypeGuid( "B3D6F875-1589-4543-9E76-5C41201B465B" )]
    [Rock.SystemGuid.BlockTypeGuid( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0" )]
    public class GeneralChat : RockBlockType
    {
        public override object GetMobileConfigurationValues()
        {
            return base.GetMobileConfigurationValues();
        }

        #region Block Actions

        [BlockAction]
        public async Task<BlockActionResult> GetChatData()
        {
            if( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized("You must be logged in to view chat.");
            }

            var person = RequestContext.CurrentPerson;

            var sharedChannelGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL.AsGuid() );
            var directMessagingChannelGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid() );

            if( !sharedChannelGroupTypeId.HasValue || !directMessagingChannelGroupTypeId.HasValue )
            {
                return ActionBadRequest( "Chat group types are not configured." );
            }

            using ( var chatHelper = new ChatHelper() )
            {
                var chatToken = await chatHelper.GetChatUserTokenAsync( person.Id );
                var chatUserKey = chatHelper.GetRockChatUserPersonKeys( new List<int> { person.Id } ).FirstOrDefault()?.ChatUserKey;

                var sharedChannelGroupStreamKey = ChatHelper.GetChatChannelTypeKey( sharedChannelGroupTypeId.Value );
                var directMessagingChannelStreamKey = ChatHelper.GetChatChannelTypeKey( directMessagingChannelGroupTypeId.Value );

                return ActionOk( new
                {
                    Token = chatToken,
                    UserId = chatUserKey,

                    // BC TODO: Should probably be passing these through the update package
                    // instead of here.
                    SharedChannelStreamKey = sharedChannelGroupStreamKey,
                    DirectMessagingChannelStreamKey = directMessagingChannelStreamKey
                } );
            }
        }
        
        #endregion

        #region Helper Classes

        #endregion
    }
}
