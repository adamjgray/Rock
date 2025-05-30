// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.Collections.Generic;

namespace Rock.ViewModels.Controls
{
    public class IconSetBag
    {
        /// <summary>
        /// A CSS class that is added with the <see cref="IconDefinitionBag.StyleClass"/>
        /// required to the icon element to cause the icon to show.
        /// </summary>
        public string StyleClassPrefix { get; set; }

        /// <summary>
        /// A list of icon definitions that are part of this icon set.
        /// </summary>
        public List<IconDefinitionBag> Icons { get; set; } = new List<IconDefinitionBag>();
    }
}
