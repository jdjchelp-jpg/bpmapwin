function node_function(node)
  if node.tags then
    if node.tags.name or node.tags.amenity or node.tags.shop then
      node:Layer('poi', 12)
      if node.tags.name then node:Attribute('name', node.tags.name) end
      if node.tags.amenity then node:Attribute('amenity', node.tags.amenity) end
      if node.tags.shop then node:Attribute('shop', node.tags.shop) end
    end
  end
end
function way_function(way)
  if way.tags and way.tags.highway then
    way:Layer('transportation', 4)
    way:Attribute('highway', way.tags.highway)
    if way.tags.name then way:Attribute('name', way.tags.name) end
  end
end
