using KargoTakip.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.Cargo.Commands.Create
{
    public class CreateCargoCommand : IRequest<CreateCargoCommandResponse>
    {
        public string SenderFullName { get; set; }
        public string SenderAddress { get; set; }
        public string SenderPhoneNumber { get; set; }

        public string ReceiverName { get; set; }
        public string ReceiverSurname { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
    }
}
