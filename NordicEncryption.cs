// Copyright (c) 2020, Ben Baker
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities.Encoders;

namespace nRF5DFUTool
{
    public class NordicEncryption
    {
        public static byte[] GetSHA256Hash(byte[] imageData)
        {
            using (SHA256Managed sha256Managed = new SHA256Managed())
                return sha256Managed.ComputeHash(imageData).Reverse().ToArray();
        }

        public static void CreateKeys(string pemPath, out ECPrivateKeyParameters privateKey, out ECPublicKeyParameters publicKey)
        {
            var pemBytes = File.ReadAllBytes(pemPath);
            var privKeyStruct = ECPrivateKeyStructure.GetInstance(pemBytes);
            var curve = ECNamedCurveTable.GetByName("P-256");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N);
            privateKey = new ECPrivateKeyParameters(privKeyStruct.GetKey(), domain);
            byte[] privateKeyBytes = privKeyStruct.GetKey().ToByteArray();
            byte[] publicKeyBytes = privKeyStruct.GetPublicKey().GetBytes();
            publicKey = new ECPublicKeyParameters("ECDSA", domain.Curve.DecodePoint(publicKeyBytes), domain);
        }

        public static byte[] SignData(ECPrivateKeyParameters privateKey, byte[] data)
        {
            try
            {
                ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                signer.Init(true, privateKey);
                signer.BlockUpdate(data, 0, data.Length);
                return signer.GenerateSignature();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Signing Failed: " + ex.ToString());
                return null;
            }
        }

        public static bool VerifySignature(byte[] publicKey, byte[] signature, byte[] data)
        {
            try
            {
                var x = publicKey.Take(32).ToArray();
                var y = publicKey.Skip(32).ToArray();

                Array.Reverse(x);
                Array.Reverse(y);

                var curve = ECNamedCurveTable.GetByName("P-256");
                var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N); //, curve.H, curve.GetSeed());
                var q = curve.Curve.CreatePoint(new BigInteger(x), new BigInteger(y));
                ECPublicKeyParameters key = new ECPublicKeyParameters(q, domain);

                ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                signer.Init(false, key);
                signer.BlockUpdate(data, 0, data.Length);
                return signer.VerifySignature(signature);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Verification failed with the error: " + ex.ToString());
                return false;
            }
        }

        public static bool VerifySignature(ECPublicKeyParameters publicKey, byte[] signature, byte[] data)
        {
            try
            {
                ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                signer.Init(false, publicKey);
                signer.BlockUpdate(data, 0, data.Length);
                return signer.VerifySignature(signature);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Verification failed with the error: " + ex.ToString());
                return false;
            }
        }

        public static byte[] DerEncodeSignature(byte[] signature)
        {
            byte[] r = signature.Take(signature.Length / 2).ToArray();
            byte[] s = signature.Skip(signature.Length / 2).ToArray();

            Array.Reverse(r);
            Array.Reverse(s);

            using (MemoryStream stream = new MemoryStream())
            {
                using (DerOutputStream der = new DerOutputStream(stream))
                {
                    Asn1EncodableVector v = new Asn1EncodableVector();
                    v.Add(new DerInteger(new BigInteger(1, r)));
                    v.Add(new DerInteger(new BigInteger(1, s)));
                    der.WriteObject(new DerSequence(v));

                    return stream.ToArray();
                }
            }
        }
    }
}
