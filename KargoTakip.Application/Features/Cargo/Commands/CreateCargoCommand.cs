using KargoTakip.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.Cargo.Commands
{
    public class CreateCargoCommand : IRequest<CreateCargoCommandResponse>
    {
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAddress { get; set; }

    }
}
