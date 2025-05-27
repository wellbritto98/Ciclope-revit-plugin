using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RevitTemplate.Models;
using System.Reflection;

namespace RevitTemplate.Services
{
    /// <summary>
    /// Serviço para gerenciamento de tokens JWT com persistência segura em arquivo JSON
    /// </summary>
    public class TokenService
    {
        private const string EncryptionKey = "RevitTemplate2025Key!@#$"; // Chave para criptografia
        private const string TokenFileName = "token.json";
        
        /// <summary>
        /// Obtém o diretório onde está localizada a DLL
        /// </summary>
        private  string GetDllDirectory()
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assemblyLocation);
        }
        
        /// <summary>
        /// Obtém o caminho completo do arquivo de token
        /// </summary>
        private  string GetTokenFilePath()
        {
            return Path.Combine(GetDllDirectory(), TokenFileName);
        }
          /// <summary>
        /// Salva o token data de forma persistente e segura em arquivo JSON
        /// </summary>
        /// <param name="tokenData">Dados do token para salvar</param>
        public  bool SaveToken(TokenData tokenData)
        {
            try
            {
                if (tokenData == null)
                {
                    ClearToken();
                    return false;
                }

                // Serializa o token data
                string jsonToken = JsonConvert.SerializeObject(tokenData, Formatting.Indented);
                
                // Criptografa o token
                string encryptedToken = EncryptString(jsonToken);
                
                // Salva no arquivo JSON
                string filePath = GetTokenFilePath();
                File.WriteAllText(filePath, encryptedToken);
                
                LogService.LogInfo($"Token salvo com sucesso em: {filePath}. Expira em: {tokenData.Expiration}");
                return true;
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao salvar token: {ex.Message}");
                return false;
            }
        }        /// <summary>
        /// Recupera o token salvo do arquivo JSON, se ainda for válido
        /// </summary>
        /// <returns>TokenData se válido, null caso contrário</returns>
        public  TokenData GetSavedToken()
        {
            try
            {
                string filePath = GetTokenFilePath();
                
                if (!File.Exists(filePath))
                {
                    LogService.LogInfo("Arquivo de token não encontrado");
                    return null;
                }
                
                string encryptedToken = File.ReadAllText(filePath);
                
                if (string.IsNullOrEmpty(encryptedToken))
                {
                    LogService.LogInfo("Arquivo de token está vazio");
                    return null;
                }

                // Descriptografa o token
                string jsonToken = DecryptString(encryptedToken);
                
                if (string.IsNullOrEmpty(jsonToken))
                {
                    LogService.LogError("Falha na descriptografia do token");
                    ClearToken();
                    return null;
                }
                
                // Deserializa o token data
                TokenData tokenData = JsonConvert.DeserializeObject<TokenData>(jsonToken);
                
                // Verifica se o token ainda é válido (não expirou)
                if (tokenData?.Expiration > DateTime.Now && tokenData.Authenticated)
                {
                    LogService.LogInfo("Token válido recuperado do arquivo");
                    return tokenData;
                }
                else
                {
                    LogService.LogInfo("Token expirado encontrado, removendo arquivo...");
                    ClearToken();
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao recuperar token: {ex.Message}");
                ClearToken(); // Remove token corrompido
                return null;
            }
        }

        /// <summary>
        /// Verifica se existe um token válido salvo
        /// </summary>
        /// <returns>True se existe um token válido</returns>
        public  bool HasValidToken()
        {
            return GetSavedToken() != null;
        }        /// <summary>
        /// Remove o arquivo de token
        /// </summary>
        public  bool ClearToken()
        {
            try
            {
                string filePath = GetTokenFilePath();
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LogService.LogInfo($"Arquivo de token removido: {filePath}");
                }
                else
                {
                    LogService.LogInfo("Arquivo de token não existia");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao remover arquivo de token: {ex.Message}");
                return false;
            }
        }        /// <summary>
        /// Obtém o token JWT atual se válido
        /// </summary>
        /// <returns>String do token JWT ou null se inválido</returns>
        public  string GetCurrentToken()
        {
            var tokenData = GetSavedToken();
            return tokenData?.Token;
        }
        
        /// <summary>
        /// Obtém informações sobre o token atual
        /// </summary>
        /// <returns>String com informações do token</returns>
        public  string GetTokenInfo()
        {
            var token = GetSavedToken();
            if (token == null)
            {
                return "Nenhum token válido encontrado";
            }

            var timeUntilExpiration = token.Expiration - DateTime.Now;
            return $"Token válido. Expira em: {timeUntilExpiration.Hours}h {timeUntilExpiration.Minutes}m";
        }

        #region Métodos de Criptografia Simples        /// <summary>
        /// Criptografa uma string usando AES com IV aleatório
        /// </summary>
        private  string EncryptString(string plainText)
        {
            try
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                
                using (Aes aes = Aes.Create())
                {
                    // Gera chave a partir da chave fixa
                    byte[] key = new byte[32]; // 256 bits
                    byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
                    Array.Copy(keyBytes, key, Math.Min(keyBytes.Length, key.Length));
                    aes.Key = key;
                    
                    // Gera IV aleatório para cada criptografia
                    aes.GenerateIV();
                    
                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        // Escreve o IV no início do stream
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                        }
                        
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro na criptografia: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Descriptografa uma string usando AES
        /// </summary>
        private  string DecryptString(string cipherText)
        {
            try
            {
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                
                using (Aes aes = Aes.Create())
                {
                    // Usa a mesma chave
                    byte[] key = new byte[32]; // 256 bits
                    byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
                    Array.Copy(keyBytes, key, Math.Min(keyBytes.Length, key.Length));
                    aes.Key = key;
                    
                    // Extrai o IV dos primeiros 16 bytes
                    byte[] iv = new byte[16];
                    Array.Copy(cipherTextBytes, 0, iv, 0, 16);
                    aes.IV = iv;
                    
                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(cipherTextBytes, 16, cipherTextBytes.Length - 16))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro na descriptografia: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
