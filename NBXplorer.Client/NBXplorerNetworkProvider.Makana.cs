using System;
using NBitcoin;
using NBitcoin.Altcoins.Elements;
using NBXplorer.DerivationStrategy;

namespace NBXplorer
{
	public partial class NBXplorerNetworkProvider
	{
		public class MakanaNBXplorerNetwork : NBXplorerNetwork
		{
			internal MakanaNBXplorerNetwork(INetworkSet networkSet, NetworkType networkType) : base(networkSet, networkType)
			{
			}

			internal override DerivationStrategyFactory CreateStrategyFactory()
			{
				var factory = base.CreateStrategyFactory();
				factory.AuthorizedOptions.Add("unblinded");
				return factory;
			}

			public override BitcoinAddress CreateAddress(DerivationStrategyBase derivationStrategy, KeyPath keyPath, Script scriptPubKey)
			{
				if (derivationStrategy.Unblinded())
				{
					return base.CreateAddress(derivationStrategy, keyPath, scriptPubKey);
				}
				var blindingPubKey = GenerateBlindingKey(derivationStrategy, keyPath).PubKey;
				return new BitcoinBlindedAddress(blindingPubKey, base.CreateAddress(derivationStrategy, keyPath, scriptPubKey));
			}

			public static Key GenerateBlindingKey(DerivationStrategyBase derivationStrategy, KeyPath keyPath)
			{
				if (derivationStrategy.Unblinded())
				{
					throw new InvalidOperationException("This derivation scheme is set to only track unblinded addresses");
				}
				var blindingKey = new Key(derivationStrategy.GetChild(keyPath).GetChild(new KeyPath("0")).GetDerivation()
					.ScriptPubKey.WitHash.ToBytes());
				return blindingKey;
			}
		}
		private void InitMakana(NetworkType networkType)
		{
			Add(new MakanaNBXplorerNetwork(NBitcoin.Altcoins.Makana.Instance, networkType)
			{
				MinRPCVersion = 150000,
				CoinType = networkType == NetworkType.Mainnet ? new KeyPath("1776'") : new KeyPath("1'"),
			});
		}

		public NBXplorerNetwork GetMKNA()
		{
			return GetFromCryptoCode(NBitcoin.Altcoins.Makana.Instance.CryptoCode);
		}
	}

	public static class MakanaDerivationStrategyOptionsExtensions
	{
		public static bool Unblinded(this DerivationStrategyBase derivationStrategyBase)
		{
			return derivationStrategyBase.AdditionalOptions.TryGetValue("unblinded", out var unblinded) is true && unblinded;
		}
	}
}
