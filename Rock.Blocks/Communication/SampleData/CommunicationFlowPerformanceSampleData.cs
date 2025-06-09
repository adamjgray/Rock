using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;

using Rock.Attribute;
using Rock.Enums.Communication;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays the performance of a particular communication flow.
    /// </summary>

    [DisplayName( "Communication Flow Performance Sample Data" )]
    [Category( "Communication" )]
    [Description( "Displays the performance of a particular communication flow sample data." )]
    [IconCssClass( "fa fa-line-chart" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "C7335E83-7B93-4AC0-AC1D-348B82E936F6" )]
    [Rock.SystemGuid.BlockTypeGuid( "FBE95153-A67E-4BF4-9FA3-E4BD9EB7584A" )]
    public class CommunicationFlowPerformanceSampleData : RockBlockType
    {
        private readonly Random _random = new Random();

        public override object GetObsidianBlockInitialization()
        {
            var currentPerson = GetCurrentPerson();

            return new
            {
                smsFromSystemPhoneNumbers = SystemPhoneNumberCache.All( false )
                    .Where( spn => spn.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    .OrderBy( spn => spn.Order )
                    .ThenBy( spn => spn.Name )
                    .ThenBy( spn => spn.Id )
                    .ToListItemBagList()
            };
        }

        [BlockAction( "GenerateRecurringFlow" )]
        public BlockActionResult GenerateRecurringFlow( string iCalendarContent, ListItemBag targetAudienceDataView, ListItemBag smsFromSystemPhoneNumber )
        {
            var maxUnsubscribeLevel = Enum.GetValues( typeof( UnsubscribeLevel ) ).Cast<UnsubscribeLevel>().Max();
            var communicationFlowService = new CommunicationFlowService( this.RockContext );

            // Construct flow and save.
            var communicationFlow = new CommunicationFlow
                {
                    CommunicationFlowCommunications = new List<CommunicationFlowCommunication>
                    {
                        new CommunicationFlowCommunication
                        {
                            CommunicationTemplate = new CommunicationTemplate
                            {
                                Name = "Sample Message 1",
                                Subject = "Welcome to the Sample Recurring Flow",
                                Message = @"!DOCTYPE html
        <html lang=""en-us"">
            <head>
                <meta charset=""utf-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                <title>Sample Message 1</title>
            </head>
            <body>
                <p>Thank you for joining our sample recurring flow!</p>
            </body>
        </html>",
                                FromEmail = "test@test.com",
                                FromName = "Sample Sender",
                                IsActive = true,
                                Version = CommunicationTemplateVersion.Beta,
                                UsageType = CommunicationTemplateUsageType.CommunicationFlows
                            },
                            Name = "Sample Message 1",
                            Order = 0
                        },

                        new CommunicationFlowCommunication
                        {
                            CommunicationTemplate = new CommunicationTemplate
                            {
                                Name = "Sample Message 2",
                                Subject = "Follow-up on the Sample Recurring Flow",
                                SMSMessage = "We hope you are enjoying the sample recurring flow! 😁",
                                SmsFromSystemPhoneNumberId = smsFromSystemPhoneNumber?.Value.AsGuidOrNull().HasValue == true ? SystemPhoneNumberCache.GetId( smsFromSystemPhoneNumber.Value.AsGuid() ) : null,
                                IsActive = true,
                                Version = CommunicationTemplateVersion.Beta,
                                UsageType = CommunicationTemplateUsageType.CommunicationFlows
                            },
                            Name = "Sample Message 2",
                            Order = 1
                        }
                    },
                    ConversionGoalTargetPercent = Convert.ToDecimal( _random.NextDouble() ),
                    ConversionGoalTimeframeInDays = _random.Next( 1, 14 ),
                    ConversionGoalType = ConversionGoalType.JoinedGroupType,
                    Description = "Sample Recurring Communication Flow",
                    ExitConditionType = ExitConditionType.ConversionAchieved,
                    Name = "Sample Recurring Communication Flow",
                    Schedule = new Schedule
                    {
                        iCalendarContent = iCalendarContent,
                    },
                    TargetAudienceDataViewId = targetAudienceDataView?.Value?.AsGuidOrNull().HasValue == true ? DataViewCache.GetId( targetAudienceDataView.Value.AsGuid() ) : null,
                    TriggerType = CommunicationFlowTriggerType.Recurring,
                };
            communicationFlowService.Add( communicationFlow );
            this.RockContext.SaveChanges();

            // Every flow instance will have the same recipients (target audience)
            // so retrieve them once at the top of instance(s) creation.
            var recipientPersonAliasIds = DataViewCache.Get( targetAudienceDataView.Value.AsGuid() ).GetEntityIds().ToList();
            var recipientPersonAliases = new PersonAliasService( this.RockContext ).GetByIds( recipientPersonAliasIds );

            // Create flow instances.
            var instanceDate = communicationFlow.Schedule.FirstStartDateTime;
            while ( instanceDate.HasValue )
            {
                // TODO JMH Keep track of the unsubscribed and converted recipients
                // so they don't get more communications PER INSTANCE.
                // For example, if a person converts in one instance, then they
                // should no longer receive communications in that instance.
                // If a future instance is started, that recipient should be added
                // back until they unsubscribe or convert again.
                var unsubscribedPersonAliasIds = new List<int>();
                var convertedPersonAliasIds = new List<int>();

                // Create the flow instance.
                var flowInstance = new CommunicationFlowInstance
                {
                    StartDate = instanceDate.Value,
                };
                communicationFlow.CommunicationFlowInstances.Add( flowInstance );
                this.RockContext.SaveChanges();

                // Add the communications to each instance.
                foreach ( var c in communicationFlow.CommunicationFlowCommunications )
                {
                    // Create an instance communication.
                    var communication = c.CommunicationTemplate.Message.IsNotNullOrWhiteSpace()
                                ? new Model.Communication
                                {
                                    CommunicationTemplate = c.CommunicationTemplate,
                                    FromEmail = c.CommunicationTemplate.FromEmail,
                                    FromName = c.CommunicationTemplate.FromName,
                                    Message = c.CommunicationTemplate.Message,
                                    Name = c.CommunicationTemplate.Name,
                                    Subject = c.CommunicationTemplate.Subject
                                }
                                : c.CommunicationTemplate.SMSMessage.IsNotNullOrWhiteSpace()
                                ? new Model.Communication
                                {
                                    CommunicationTemplate = c.CommunicationTemplate,
                                    SmsFromSystemPhoneNumberId = c.CommunicationTemplate.SmsFromSystemPhoneNumberId,
                                    SMSMessage = c.CommunicationTemplate.SMSMessage,
                                    Name = c.CommunicationTemplate.Name
                                }
                                : null;
                    flowInstance.CommunicationFlowInstanceCommunications.Add( new CommunicationFlowInstanceCommunication
                    {
                        Communication = communication,
                        CommunicationFlowCommunication = c
                    } );
                    this.RockContext.SaveChanges();

                    // Add recipients to the communication.
                    foreach ( var personAlias in recipientPersonAliases.Where( pa => !unsubscribedPersonAliasIds.Contains( pa.Id ) && !convertedPersonAliasIds.Contains( pa.Id ) ) )
                    {
                        // Every so often a recipient may unsubscribe.
                        // TODO JMH Unsubscribed people should not appear in following
                        // communications for the instance (or future instances/flows?).
                        var causedUnsubscribe = _random.NextDouble() > 0.95;
                        var unsubscribeDateTime = causedUnsubscribe ? RockDateTime.Now : default;
                        var unsubscribeLevel = causedUnsubscribe ? ( UnsubscribeLevel ) _random.Next( maxUnsubscribeLevel.ConvertToInt() + 1 ) : default;

                        communication.Recipients.Add( new CommunicationRecipient
                        {
                            PersonAlias = personAlias,
                            CausedUnsubscribe = causedUnsubscribe,
                            UnsubscribeDateTime = unsubscribeDateTime,
                            UnsubscribeLevel = unsubscribeLevel
                        } );

                        if ( causedUnsubscribe )
                        {
                            unsubscribedPersonAliasIds.Add( personAlias.Id );
                        }
                        else
                        {
                            // Let's see if this person converted!
                            var didConvert = _random.NextDouble() > 0.95;
                            if ( didConvert )
                            {
                                // Add a conversion history record to the flow instance.
                                flowInstance.CommunicationFlowInstanceConversionHistories.Add( new CommunicationFlowInstanceConversionHistory
                                {
                                    PersonAlias = personAlias,
                                    Date = RockDateTime.Now,
                                    CommunicationFlowCommunication = c
                                } );
                                convertedPersonAliasIds.Add( personAlias.Id );
                            }
                        }

                        this.RockContext.SaveChanges();
                    }
                }

                // Move to the next date.
                instanceDate = communicationFlow.Schedule.GetNextStartDateTime( instanceDate.Value.AddDays( 1 ) );
            }

            return ActionOk( new
            {
                communicationFlow.Id,
                communicationFlow.Guid
            } );
        }
    }
}
