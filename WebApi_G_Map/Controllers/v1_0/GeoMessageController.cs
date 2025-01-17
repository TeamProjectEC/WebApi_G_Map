﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi_G_Map.Data;
using WebApi_G_Map.Models;


namespace WebApi_G_Map.Controllers.v1_0
{
    /// <summary>
    /// Controller för version 1.0
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class GeoMessageController : ControllerBase
    {
        private readonly GeoMessageContext _context;
        private readonly UserManager<GeoUser> _userManager;

        public GeoMessageController(GeoMessageContext context, UserManager<GeoUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        ///  Vissar alla geo meddelanden som skickade med POST 
        /// </summary>
        /// <returns> <see cref="IEnumerable{GeoMessageV1}"/></returns>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GeoMessageV1>>> GetMessage()
        {

            List<GeoMessageV2> msgv2 = await _context.GeoMessagesV2.Include(m => m.Message).ToListAsync();
            List<GeoMessageV1> msgv1 = await _context.GeoMessagesV1.Where(x => !string.IsNullOrEmpty(x.message)).ToListAsync();

            foreach (var item in msgv2)
            {
                GeoMessageV1 geo = new GeoMessageV1
                {
                    message = item.Message.body,
                    latitude = item.latitude,
                    longitude = item.longitude
                };
                msgv1.Add(geo);

            }
            return (msgv1);
           
        }

        /// <summary>
        /// Hitta meddelendet med ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="GeoMessageV1"/></returns>

        [HttpGet("{id}")]
        public async Task<ActionResult<GeoMessageV1>> GetMessage(int id)
        {
            var ms = await _context.GeoMessagesV1.FindAsync(id);

            if (ms == null)
            {
                return NotFound();
            }

            return (ms);
        }

        /// <summary>
        ///  Skicka ett meddelande (med Login)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GeoMessageV1>> PostMessage(GeoMessageV1 message)
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.Users
                .Where(u => u.Id == userId)
                .Include(m => m.GeoMessagesV1)
                .FirstOrDefault();

            user.GeoMessagesV1.Add(message);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMessage", new { id = message.Id }, message);

        }

    }
}