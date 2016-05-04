/* ========================================================================
 * Copyright (c) 2005-2013 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Reciprocal Community License ("RCL") Version 1.00
 * 
 * Unless explicitly acquired and licensed from Licensor under another 
 * license, the contents of this file are subject to the Reciprocal 
 * Community License ("RCL") Version 1.00, or subsequent versions 
 * as allowed by the RCL, and You may not copy or use this file in either 
 * source code or executable form, except in compliance with the terms and 
 * conditions of the RCL.
 * 
 * All software distributed under the RCL is provided strictly on an 
 * "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
 * AND LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT 
 * LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
 * PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RCL for specific 
 * language governing rights and limitations under the RCL.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/RCL/1.00/
 * ======================================================================*/

using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Opc.Ua.Bindings;

namespace Opc.Ua
{
    /// <summary>
    /// An object used by clients to access a UA discovery service.
    /// </summary>
    public partial class RegistrationClient
    {
        #region Constructors
        /// <summary>
        /// Creates a binding for to use for discovering servers.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="description">The description.</param>
        /// <param name="endpointConfiguration">The endpoint configuration.</param>
        /// <param name="bindingFactory">The binding factory.</param>
        /// <param name="instanceCertificate">The instance certificate.</param>
        /// <returns></returns>
        public static RegistrationClient Create( 
            ApplicationConfiguration configuration,
            EndpointDescription      description,
            EndpointConfiguration    endpointConfiguration,
            BindingFactory           bindingFactory,
            X509Certificate2         instanceCertificate)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (description == null) throw new ArgumentNullException("description");

            ITransportChannel channel = RegistrationChannel.Create(
                configuration, 
                description, 
                endpointConfiguration, 
                instanceCertificate,
                new ServiceMessageContext());

            return new RegistrationClient(channel);
        }

        /// <summary>
        /// Creates a binding for to use for discovering servers.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="description">The description.</param>
        /// <param name="endpointConfiguration">The endpoint configuration.</param>
        /// <param name="bindingFactory">The binding factory.</param>
        /// <param name="instanceCertificates">The instance certificate chain.</param>
        /// <returns></returns>
        /*public static RegistrationClient Create(
            ApplicationConfiguration configuration,
            EndpointDescription description,
            EndpointConfiguration endpointConfiguration,
            BindingFactory bindingFactory,
            X509Certificate2Collection instanceCertificates )
        {
            if (configuration == null) throw new ArgumentNullException( "configuration" );
            if (description == null) throw new ArgumentNullException( "description" );

            ITransportChannel channel = RegistrationChannel.Create(
                configuration,
                description,
                endpointConfiguration,
                instanceCertificates,
                new ServiceMessageContext() );

            return new RegistrationClient( channel );
        }*/
        #endregion
    }
    
    /// <summary>
    /// A channel object used by clients to access a UA discovery service.
    /// </summary>
    public partial class RegistrationChannel
    {
        #region Constructors
        /// <summary>
        /// Creates a new transport channel that supports the IRegistrationChannel service contract.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="description">The description for the endpoint.</param>
        /// <param name="endpointConfiguration">The configuration to use with the endpoint.</param>
        /// <param name="clientCertificate">The client certificate.</param>
        /// <param name="messageContext">The message context to use when serializing the messages.</param>
        /// <returns></returns>
        public static ITransportChannel Create(
            ApplicationConfiguration configuration,
            EndpointDescription description,
            EndpointConfiguration endpointConfiguration,
            X509Certificate2 clientCertificate,
            ServiceMessageContext messageContext)
        {
            // create a UA binary channel.
            ITransportChannel channel = CreateUaBinaryChannel(
                configuration,
                description,
                endpointConfiguration,
                clientCertificate,
                messageContext);

            // create a WCF XML channel.
            if (channel == null)
            {
                Uri endpointUrl = new Uri(description.EndpointUrl);
                BindingFactory bindingFactory = BindingFactory.Create(configuration, messageContext);
                Binding binding = bindingFactory.Create(endpointUrl.Scheme, description, endpointConfiguration);

                RegistrationChannel wcfXmlChannel = new RegistrationChannel();

                wcfXmlChannel.Initialize(
                    configuration,
                    description,
                    endpointConfiguration,
                    binding,
                    clientCertificate,
                    null);

                channel = wcfXmlChannel;
            }

            return channel;
        }

        /// <summary>
        ///  Creates a new transport channel that supports the IRegistrationChannel service contract.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="description">The description for the endpoint.</param>
        /// <param name="endpointConfiguration">The endpoint configuration.</param>
        /// <param name="clientCertificates">The client certificates.</param>
        /// <param name="messageContext">The message context to use when serializing the messages.</param>
        /// <returns></returns>
        /*public static ITransportChannel Create(
            ApplicationConfiguration configuration,
            EndpointDescription description,
            EndpointConfiguration endpointConfiguration,
            X509Certificate2Collection clientCertificates,
            ServiceMessageContext messageContext )
        {
            // create a UA binary channel.
            ITransportChannel channel = CreateUaBinaryChannel(
                configuration,
                description,
                endpointConfiguration,
                clientCertificates,
                messageContext );

            // create a WCF XML channel.
            if (channel == null)
            {
                Uri endpointUrl = new Uri( description.EndpointUrl );
                BindingFactory bindingFactory = BindingFactory.Create( configuration, messageContext );
                Binding binding = bindingFactory.Create( endpointUrl.Scheme, description, endpointConfiguration );

                RegistrationChannel wcfXmlChannel = new RegistrationChannel();

                wcfXmlChannel.Initialize(
                    configuration,
                    description,
                    endpointConfiguration,
                    binding,
                    clientCertificates,
                    null );

                channel = wcfXmlChannel;
            }

            return channel;
        }
         */

        /// <summary>
        /// Creates a discovery channel that uses the specified binding.
        /// </summary>
        [Obsolete("Must use the version that returns a ITransportChannel object.")]
        public static RegistrationChannel Create(
            ApplicationConfiguration configuration,
            EndpointDescription      description,
            EndpointConfiguration    endpointConfiguration,
            Binding                  binding,
            X509Certificate2         clientCertificate,
            string                   configurationName)
        {
            RegistrationChannel channel = new RegistrationChannel();

            channel.Initialize(
                configuration,
                description,
                endpointConfiguration,
                binding,
                clientCertificate,
                configurationName);

            return channel;
        }
        #endregion
    } 
}
