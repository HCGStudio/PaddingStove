module.exports = {
  importOrderSeparation: true,
  importOrderSortSpecifiers: true,
  importOrder: ["^[./]"],
  plugins: [require.resolve("@trivago/prettier-plugin-sort-imports")],
};
