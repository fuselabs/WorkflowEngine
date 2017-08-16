// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace WorkflowEngine
{
    [DebuggerDisplay("Id: {" + nameof(Id) + "}")]
    public abstract class ObjectId
    {
        private Guid _id;

        protected ObjectId()
        {
            _id = Guid.NewGuid();
        }

        protected ObjectId(Guid guid)
        {
            _id = guid;
        }

        protected ObjectId(string guidString)
        {
            _id = !string.IsNullOrEmpty(guidString) ? Guid.Parse(guidString) : Guid.NewGuid();
        }

        public string Id
        {
            get => _id.ToString();
            set => _id = new Guid(value); // We must keep this public setter so that JSON.net can deserialize these Ids
        }

        [JsonIgnore]
        public Guid IdGuid => _id;

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectId);
        }

        public bool Equals(ObjectId other)
        {
            if (other == null)
            {
                return false;
            }

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
