// ***********************************************************************
// Assembly         : ACBr.Net.NFSe
// Author           : Rafael Dias
// Created          : 01-31-2016
//
// Last Modified By : Rafael Dias
// Last Modified On : 06-01-2018
// ***********************************************************************
// <copyright file="ProviderManager.cs" company="ACBr.Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2016-2018 Grupo ACBr.Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	 The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary></summary>
// ***********************************************************************

using ACBr.Net.Core;
using ACBr.Net.Core.Extensions;
using ACBr.Net.NFSe.Configuracao;
using ACBr.Net.NFSe.Providers.Sigiss;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ACBr.Net.NFSe.Providers
{
    /// <summary>
    /// Classe respons�vel por criar uma nova instancia do provedor
    /// </summary>
    public static class ProviderManager
    {
        #region Constructors

        static ProviderManager()
        {
            Municipios = new List<ACBrMunicipioNFSe>();
            Providers = new Dictionary<NFSeProvider, Type>
            {
                {NFSeProvider.Abaco, typeof(ProviderAbaco)},
                {NFSeProvider.BHISS, typeof(ProviderBHISS)},
                {NFSeProvider.Betha, typeof(ProviderBetha)},
                {NFSeProvider.Betha2, typeof(ProviderBetha2)},
                {NFSeProvider.Coplan, typeof(ProviderCoplan)},
                {NFSeProvider.DBSeller, typeof(ProviderDBSeller)},
                {NFSeProvider.DSF, typeof(ProviderDSF)},
                {NFSeProvider.Equiplano, typeof(ProviderEquiplano)},
                {NFSeProvider.Fiorilli, typeof(ProviderFiorilli)},
                {NFSeProvider.FissLex, typeof(ProviderFissLex)},
                {NFSeProvider.Ginfes, typeof(ProviderGinfes)},
                {NFSeProvider.ISSNet, typeof(ProviderISSNet)},
                {NFSeProvider.NFeCidades, typeof(ProviderNFeCidades)},
                {NFSeProvider.NotaCarioca, typeof(ProviderNotaCarioca)},
                {NFSeProvider.Pronim2, typeof(ProviderPronim2)},
                {NFSeProvider.SaoPaulo, typeof(ProviderSaoPaulo)},
                {NFSeProvider.SmarAPDABRASF, typeof(ProviderSmarAPDABRASF)},
                {NFSeProvider.Vitoria, typeof(ProviderVitoria)},
                {NFSeProvider.WebIss, typeof(ProviderWebIss)},
                {NFSeProvider.WebIss2, typeof(ProviderWebIss2)},
                {NFSeProvider.Sigiss, typeof(ProviderSigiss)},
                {NFSeProvider.Conam, typeof(ProviderCONAM)},
                {NFSeProvider.Goiania, typeof(ProviderGoiania)},
                {NFSeProvider.ISSe, typeof(ProviderISSe)},
                {NFSeProvider.SimplISS, typeof(ProviderSimplISS)},
                {NFSeProvider.SpeedGov, typeof(ProviderSpeedGov)}
            };

            Load();
        }

        #endregion Constructors

        #region Propriedades

        /// <summary>
        /// Municipios cadastrados no ACBrNFSe
        /// </summary>
        /// <value>Os municipios</value>
        public static List<ACBrMunicipioNFSe> Municipios { get; }

        /// <summary>
        /// Provedores cadastrados no ACBrNFSe
        /// </summary>
        /// <value>Os provedores</value>
        public static Dictionary<NFSeProvider, Type> Providers { get; }

        #endregion Propriedades

        #region Methods

        #region Public

        /// <summary>
        /// Salva o arquivo de cidades.
        /// </summary>
        /// <param name="path">Caminho para salvar o arquivo</param>
        public static void Save(string path = "Municipios.nfse")
        {
            Guard.Against<ArgumentNullException>(path == null, "Path invalido.");

            if (File.Exists(path)) File.Delete(path);

            using (var fileStream = new FileStream(path, FileMode.CreateNew))
            {
                Save(fileStream);
            }
        }

        /// <summary>
        /// Salva o arquivo de cidades.
        /// </summary>
        /// <param name="stream">O stream.</param>
        public static void Save(Stream stream)
        {
            var formatter = new DataContractSerializer(typeof(MunicipiosNFSe));
            formatter.WriteObject(stream, new MunicipiosNFSe { Municipios = Municipios.OrderBy(x => x.Nome).ToArray() });
        }

        /// <summary>
        /// Carrega o arquivo de cidades.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="clean">if set to <c>true</c> [clean].</param>
        public static void Load(string path = "", bool clean = true)
        {
            byte[] buffer = null;
            if (path.IsEmpty())
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("ACBr.Net.NFSe.Resources.Municipios.nfse"))
                {
                    if (stream != null)
                    {
                        buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                    }
                }
            }
            else if (File.Exists(path))
            {
                buffer = File.ReadAllBytes(path);
            }

            Guard.Against<ArgumentException>(buffer == null, "Arquivo de cidades n�o encontrado");

            using (var stream = new MemoryStream(buffer))
            {
                Load(stream, clean);
            }
        }

        /// <summary>
        /// Carrega o arquivo de cidades.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="clean">if set to <c>true</c> [clean].</param>
        public static void Load(Stream stream, bool clean = true)
        {
            Guard.Against<ArgumentException>(stream == null, "Arquivo de cidades n�o encontrado");

            var formatter = new DataContractSerializer(typeof(MunicipiosNFSe));
            var municipiosNFSe = (MunicipiosNFSe)formatter.ReadObject(stream);

            if (clean) Municipios.Clear();
            Municipios.AddRange(municipiosNFSe.Municipios);
        }

        /// <summary>
        /// Retorna o provedor para o municipio nas configura��es informadas.
        /// </summary>
        /// <param name="config">A configura��o.</param>
        /// <returns>Provedor NFSe.</returns>
        public static ProviderBase GetProvider(ConfigNFSe config)
        {
            var municipio = Municipios.SingleOrDefault(x => x.Codigo == config.WebServices.CodigoMunicipio);
            Guard.Against<ACBrException>(municipio == null, "Provedor para esta cidade n�o implementado ou n�o especificado!");

            // ReSharper disable once PossibleNullReferenceException
            var providerType = Providers[municipio.Provedor];
            Guard.Against<ACBrException>(providerType == null, "Provedor n�o encontrado!");
            Guard.Against<ACBrException>(!CheckBaseType(providerType), "Classe base do provedor incorreta!");

            // ReSharper disable once AssignNullToNotNullAttribute
            return (ProviderBase)Activator.CreateInstance(providerType, config, municipio);
        }

        #endregion Public

        #region Private

        private static bool CheckBaseType(Type providerType)
        {
            return typeof(ProviderBase).IsAssignableFrom(providerType) ||
                   typeof(ProviderABRASF).IsAssignableFrom(providerType) ||
                   typeof(ProviderABRASF201).IsAssignableFrom(providerType) ||
                   typeof(ProviderABRASF202).IsAssignableFrom(providerType) ||
                   typeof(ProviderABRASF204).IsAssignableFrom(providerType);
        }

        #endregion Private

        #endregion Methods
    }
}