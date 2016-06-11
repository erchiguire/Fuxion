﻿using Fuxion.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity
{
    public class IdentityKeyValueRepositoryValue : IKeyValueEntry<string, IIdentity>
    {
        public string Key { get; set; }
        public IIdentity Value { get; set; }
    }
    public class IdentityManager
    {
        public IdentityManager(IPasswordProvider passwordProvider, IKeyValueRepository<IdentityKeyValueRepositoryValue,string, IIdentity> repository)//, IPrincipalProvider principalProvider)
        {
            PasswordProvider = passwordProvider;
            Repository = repository;
            //PrincipalProvider = principalProvider;
        }
        public bool IsAuthenticated { get { return Current != null; } }
        //Dictionary<string, IIdentity> cache = new Dictionary<string, IIdentity>();
        public IIdentity Current { get; private set; }
        //public Action<string, bool> Console { get; set; }
        public static IPrincipalProvider PrincipalProvider { get; private set; }
        public IPasswordProvider PasswordProvider { get; private set; }
        public IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity> Repository { get; private set; }
        //private void WriteConsole(string message, bool endOfMessage) { if (Console != null) Console(message, endOfMessage); }
        public bool Login(string username, string password, bool changeCurrentIdentity = true)
        {
            Printer.Print($"Validando credenciales\r\n   Usuario: {username}\r\n   Contraseña: {password}\r\n");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Printer.Print($"Resultado: NO VALIDO - El nombre de usuario o la contraseña es NULL");
                return false;
            }
            var ide = Repository.Find(username);
            if (ide == null)
            {
                Printer.Print($"Resultado: NO VALIDO - No se ha encontrado una identidad con ese nombre de usuario");
                return false;
            }
            var res = PasswordProvider.Verify(password, ide.PasswordHash, ide.PasswordSalt);
            if (changeCurrentIdentity) Current = ide;
            if (res)
            {
                Printer.Print($"Resultado: VALIDO");
                //PrincipalProvider.SetPrincipal(new FuxionPrincipal(ide));
            }
            return res;
        }
        public void Logout()
        {
            Current = null;
        }
    }
    public interface IPasswordProvider
    {
        bool Verify(string password, byte[] hash, byte[] salt);
        void Generate(string password, out byte[] salt, out byte[] hash);
    }
    public interface IStorageProvider
    {
        IQueryable<IIdentity> Identities { get; }
    }
}
